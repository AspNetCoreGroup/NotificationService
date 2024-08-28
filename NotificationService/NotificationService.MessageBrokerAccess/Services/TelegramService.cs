using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Abstractions.BrokersServices;
using NotificationService.Domain.Abstractions.Repositories;
using NotificationService.Domain.Entities;
using NotificationService.MessageBrokerAccess.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.MessageBrokerAccess;

public class TelegramService : ITelegramService, IDisposable
{
    private readonly ILogger<TelegramService> _logger;
    private readonly IServiceProvider _serviceProvider;
    // private readonly IMessageRepository _messageRepository;
    // private readonly IUserRepository _userRepository;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    
    private const string SendEventQueue = "telegram_send_message_queue";
    private const string SendRegistrationQueue = "telegram_send_message_queue";
    private const string GetStatusQueue = "telegram_get_message_status_queue";
    private const string GetRegistrationQueue = "telegram_get_registration_queue";

    public TelegramService(
        ILogger<TelegramService> logger,
        // IMessageRepository messageRepository,
        // IUserRepository userRepository,
        IServiceProvider serviceProvider
        )
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        // _messageRepository = messageRepository;
        // _userRepository = userRepository;
        
        var uri = Environment.GetEnvironmentVariable("ConnectionStrings_Telegram_RabbitMQ");

        if (uri is null)
        {
            _logger.LogCritical("No uri for telegram rabbitmq");
            throw new Exception("No uri for telegram rabbitmq");
        }
        
        var factory = new ConnectionFactory() { Uri = new Uri(uri) };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.QueueDeclare(queue: SendEventQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: SendRegistrationQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: GetStatusQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: GetRegistrationQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }

    public async Task SendEvent(Event message)
    {
        using var scope = _serviceProvider.CreateScope();
        var messageRepository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
        
        await messageRepository.UpdateMessageDeliveryStatusAsync(message.MessageId, "PENDING");

        var messageString = JsonSerializer.Serialize(message);
        
        var body = Encoding.UTF8.GetBytes(messageString);
        _channel.BasicPublish(exchange: "", routingKey: SendEventQueue, basicProperties: null, body: body);
        
        _logger.LogDebug("Sent: {Message}", messageString);
    }
    
    public async Task SendUserForRegistration(User user)
    {
        var message = JsonSerializer.Serialize(user);
        
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: "", routingKey: SendRegistrationQueue, basicProperties: null, body: body);
        
        _logger.LogDebug("Sent: {Message}", message);
    }

    public void StartListeningForResponses()
    {
        InitMessageStatusQueue();
        InitTelegramAccountRegistrationQueue();
    }

    private void InitMessageStatusQueue()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var responseMessage = Encoding.UTF8.GetString(body);
            
            await ProcessMessageForStatus(responseMessage);
        };

        _channel.BasicConsume(queue: GetStatusQueue, autoAck: true, consumer: consumer);
    }
    
    private void InitTelegramAccountRegistrationQueue()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var responseMessage = Encoding.UTF8.GetString(body);
            
            await ProcessMessageForRegistration(responseMessage);
        };

        _channel.BasicConsume(queue: GetRegistrationQueue, autoAck: true, consumer: consumer);
    }

    private async Task ProcessMessageForStatus(string message)
    {
        var messageStatus = JsonSerializer.Deserialize<MessageStatus>(message);

        if (messageStatus is null)
        {
            _logger.LogWarning("MessageStatus from broker is null");
            return;
        }
        
        using var scope = _serviceProvider.CreateScope();
        var messageRepository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
        
        var statusCode = messageStatus.IsSuccess ? "SEND" : "FAIL";
        await messageRepository.UpdateMessageDeliveryStatusAsync(messageStatus.MessageId, statusCode);
    }
    
    private async Task ProcessMessageForRegistration(string message)
    {
        var registration = JsonSerializer.Deserialize<UserTelegramChatRegistration>(message);

        if (registration is null)
        {
            _logger.LogWarning("UserTelegramChatRegistration from broker is null");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        
        await userRepository.UpdateTelegramChatIdAsync(registration.UserId, registration.TelegramChatId);
    }
}