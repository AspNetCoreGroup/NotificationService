using System.Text;
using NotificationService.Domain.Entities;
using RabbitMQ.Client;

namespace NotificationService.MessageBrokerAccess;

public class NotificationSender
{
    private readonly IModel _channel;

    public NotificationSender(IModel channel)
    {
        _channel = channel;
    }

    public void SendNotifications(List<User> users, string message)
    {
        foreach (var user in users)
        {
            // Отправка уведомления в соответствующую очередь (TelegramService, EmailService)
            var messageBody = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "", routingKey: "TelegramQueue", basicProperties: null, body: messageBody);
        }
    }
}