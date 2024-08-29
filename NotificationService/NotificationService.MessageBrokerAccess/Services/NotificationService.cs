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

    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;

    public NotificationService(
        ILogger<NotificationService> logger,
        IServiceProvider serviceProvider
        )
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        
        var uri = Environment.GetEnvironmentVariable("ConnectionStrings_Notification_RabbitMQ");

        if (uri is null)
        {
            _logger.LogCritical("No uri for notification rabbitmq");
            throw new Exception("No uri for notification rabbitmq");
        }
        
        var factory = new ConnectionFactory() { Uri = new Uri(uri) };
        
        _queueName = "NotificationMessages";
        
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }

    public void StartListening()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            var param = PropertiesToDictionary(ea.BasicProperties);

            var args = new MessageRecievedEventArgs()
            {
                QueueName = _queueName,
                Message = message,
                Param = param,
                Failed = false,
                Hadled = false,
                Rejected = false,
                Resended = false,
            };
            
            await ProcessMessage(args);
        };

        _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
    }

    private async Task ProcessMessage(MessageRecievedEventArgs args)
    {
        var notification = ProcessArgs(args);
        
        // var notification = JsonSerializer.Deserialize<NotificationMessage>(message);

        if (notification is null)
        {
            _logger.LogWarning("EventsMessage from notification broker is null");
            return;
        }
        
        using var scope = _serviceProvider.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var messageRepository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
        var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramService>();
        
        var userId = notification.UserID;
        
        // Получение юзера и как его уведомлять
        var user = await userRepository.GetUserByIdAsync(userId);

        if (user is null)
        {
            _logger.LogWarning("Dont get User with id {Id} from db", userId);
            return;
        }

        var subs = await userRepository.GetUserSubscriptionsAsync(userId);
    
        // По типам как уведомлять формирование сообщение и отправка через определенный брокер
        foreach (var sub in subs)
        {
            MessageTrackingDto messageTracking = new()
            {
                MessageDeliveryStatusCode = "PENDING",
                SubscriptionTypeCode = sub,
                MessageText = JsonSerializer.Serialize(notification),
                UserId = userId
            };
            
            var messageId = await messageRepository.AddMessageAsync(messageTracking);
            _logger.LogInformation("Added message with id {MessageId} to db", messageId);
            
            switch (sub)
            {
                case "TELEGRAM":
                    var notificationEvent = new Event
                    {
                        UserId = userId,
                        Type = notification.NotificationsType,
                        MessageId = messageId,
                        MessageParams = notification.Params?.ToArray() ?? Enumerable.Empty<MessageParam>().ToArray()
                    };
                    _logger.LogInformation("Send message {MessageId} to telegram broker", messageId);
                    await telegramService.SendEvent(notificationEvent);
                    break;
                case "EMAIL":
                    _logger.LogDebug("No email service");
                    break;
                default:
                    _logger.LogInformation("UpdateMessageDeliveryStatus to message {MessageId} with FAIL", 
                        messageId);
                    await messageRepository.UpdateMessageDeliveryStatusAsync(messageId, "FAIL");
                    break;
            }
        }
    }

    private NotificationMessage? ProcessArgs(MessageRecievedEventArgs args)
    {
        try
        {
            _logger.LogInformation("Get message from queue \"{queueName}\" - \"{message}\".", args.QueueName,
                args.Message);
            
            var r = JsonSerializer.Deserialize<DataEventMessage<NotificationMessage>>(args.Message);
            return r?.Data;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while ProcessArgs with message - \"{message}\".", args.Message);
        }
        
        return null;
    }
    
    private static Dictionary<string, string> PropertiesToDictionary(IBasicProperties properties)
    {
        return new Dictionary<string, string>()
        {
            { "AppId", properties.AppId },
            { "ClusterId", properties.ClusterId },
            { "ContentEncoding", properties.ContentEncoding },
            { "ContentType", properties.ContentType },
            { "CorrelationId", properties.CorrelationId },
            { "DeliveryMode", properties.DeliveryMode.ToString() },
            { "Expiration", properties.Expiration },
            { "MessageId", properties.MessageId },
            { "Persistent", properties.Persistent.ToString() },
            { "Priority", properties.Priority.ToString() },
            { "ReplyTo", properties.ReplyTo },
            { "Type", properties.Type },
            { "UserId", properties.UserId }
        };
    }
}