using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Api;

public class MessageListener
{
    private readonly IModel _channel;

    public MessageListener(IModel channel)
    {
        _channel = channel;
    }

    public void StartListening(string queueName)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            ProcessMessage(message);
        };
        _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
    }

    private void ProcessMessage(string message)
    {
        // Передать сообщение в MessageProcessor
    }
}