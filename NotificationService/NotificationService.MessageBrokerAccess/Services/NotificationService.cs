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

public class NotificationService : INotificationService, IDisposable
{
    private readonly ILogger<NotificationService> _logger;

    private readonly IServiceProvider _serviceProvider;
    // private readonly ITelegramService _telegramService;
    // private readonly IUserRepository _userRepository;
    // private readonly IMessageRepository _messageRepository;

    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;

    public NotificationService(
        ILogger<NotificationService> logger,
        // ITelegramService telegramService,
        // IUserRepository userRepository,
        // IMessageRepository messageRepository,
        IServiceProvider serviceProvider
        )
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        // _telegramService = telegramService;
        // _userRepository = userRepository;
        // _messageRepository = messageRepository;
        
        var uri = Environment.GetEnvironmentVariable("ConnectionStrings_Notification_RabbitMQ");

        if (uri is null)
        {
            _logger.LogCritical("No uri for notification rabbitmq");
            throw new Exception("No uri for notification rabbitmq");
        }
        
        var factory = new ConnectionFactory() { Uri = new Uri(uri) };
        
        string queueName = "";
        
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _queueName = queueName;
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }

    public void StartListening()
    {
        _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            await ProcessMessage(message);
        };

        _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
    }

    private async Task ProcessMessage(string message)
    {
        // TODO
        // Получение уведомления
        var events = JsonSerializer.Deserialize<EventsMessage>(message);

        if (events is null)
        {
            _logger.LogWarning("EventsMessage from notification broker is null");
            return;
        }
        
        using var scope = _serviceProvider.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var messageRepository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
        var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramService>();

        foreach (var e in events.Events)
        {
            var userId = e.UserId;
            
            // Получение юзера и как его уведомлять
            var user = await userRepository.GetUserByIdAsync(userId);

            if (user is null)
            {
                _logger.LogWarning("Dont get User with id {Id} from db", userId);
                continue;
            }

            var subs = await userRepository.GetUserSubscriptionsAsync(userId);
        
            // По типам как уведомлять формирование сообщение и отправка через определенный брокер
            foreach (var sub in subs)
            {
                MessageTrackingDto messageTracking = new()
                {
                    MessageDeliveryStatusCode = "PENDING",
                    SubscriptionTypeCode = sub,
                    MessageText = JsonSerializer.Serialize(e),
                    UserId = userId
                };
                
                var messageId = await messageRepository.AddMessageAsync(messageTracking);
                
                switch (sub)
                {
                    case "TELEGRAM":
                        await telegramService.SendEvent(e);
                        break;
                    case "EMAIL":
                        _logger.LogDebug("No email service");
                        break;
                    default:
                        await messageRepository.UpdateMessageDeliveryStatusAsync(messageId, "FAIL");
                        break;
                }
            }
        }
    }
}