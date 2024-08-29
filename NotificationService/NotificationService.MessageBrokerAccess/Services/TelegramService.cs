using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Abstractions.BrokersServices;
using NotificationService.Domain.Abstractions.Repositories;
using NotificationService.Domain.Dtos;
using NotificationService.MessageBrokerAccess.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.MessageBrokerAccess.Services;

public class TelegramService : ITelegramService, IDisposable
{
    private readonly ILogger<TelegramService> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    private readonly IConnection _connection;
    private readonly IModel _channel;
    
    private const string SendEventQueue = "telegram_send_message_queue";
    private const string SendRegistrationQueue = "telegram_send_registration_queue";
    private const string GetStatusQueue = "telegram_get_message_status_queue";
    private const string GetRegistrationQueue = "telegram_get_registration_queue";

    public TelegramService(
        ILogger<TelegramService> logger,
        IServiceProvider serviceProvider
        )
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        
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
        
        _logger.LogDebug("SendEvent: {Message}", messageString);
    }
    
    public async Task SendUserForRegistration(AuthUser user)
    {
        var message = JsonSerializer.Serialize(user);
        
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: "", routingKey: SendRegistrationQueue, basicProperties: null, body: body);
        
        _logger.LogDebug("SendUserForRegistration: {Message}", message);
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
        _logger.LogInformation("UpdateMessageDeliveryStatus to message {MessageId} with statusCode {StatusCode}", 
            messageStatus.MessageId, statusCode);
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
        
        _logger.LogInformation("UpdateTelegramChatIdAsync to user {UserId} to TelegramChatId {TelegramChatId}", 
            registration.UserId, registration.TelegramChatId);
        await userRepository.UpdateTelegramChatIdAsync(registration.UserId, registration.TelegramChatId);
    }
}