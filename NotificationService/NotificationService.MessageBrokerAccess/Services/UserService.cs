using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Domain.Abstractions.BrokersServices;
using NotificationService.Domain.Abstractions.Repositories;
using NotificationService.Domain.Dtos;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Settings;
using NotificationService.MessageBrokerAccess.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.MessageBrokerAccess;

public class UserService : IUserService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;

    private readonly ILogger<UserService> _logger;
    // private readonly IUserRepository _userRepository;
    // private readonly ITelegramService _telegramService;
    private readonly IServiceProvider _serviceProvider;

    public UserService(
        ILogger<UserService> logger,
        // IUserRepository userRepository,
        // ITelegramService telegramService,
        IOptions<UserRabbitMqConfiguration> config,
        IServiceProvider serviceProvider
    )
    {
        _logger = logger;
        
        // _userRepository = userRepository;
        // _telegramService = telegramService;
        _serviceProvider = serviceProvider;
        
        var uri = Environment.GetEnvironmentVariable("ConnectionStrings_User_RabbitMQ");

        if (uri is null)
        {
            _logger.LogCritical("No uri for user rabbitmq");
            throw new Exception("No uri for user rabbitmq");
        }
        
        
        string queueName = "user_queue";
        
        var factory = new ConnectionFactory() { Uri = new Uri(uri) };
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
        _logger.LogInformation("Get message from user broker: {Message}", message);
        
        UserDto? userFromBroker;

        try
        {
            userFromBroker = JsonSerializer.Deserialize<UserDto>(message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error trying to deserialize user from broker");
            return;
        }

        if (userFromBroker is null)
        {
            _logger.LogWarning("User from broker is null");
            return;
        }
        
        var user = new User()
        {
            Id = userFromBroker.UserID,
            Name = userFromBroker.UserLogin,
            Telegram = userFromBroker.NotificationTelegramID,
            Email = userFromBroker.NotificationEmail 
        };

        using var scope = _serviceProvider.CreateScope();

        try
        {
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        
            _logger.LogInformation("Add user to db with id {UserId}", user.Id);
            await userRepository.AddUserAsync(user);

            if (!string.IsNullOrEmpty(user.Telegram))
            {
                _logger.LogInformation("Add to user {UserId} telegram sub", user.Id);
                await userRepository.AddSubscriptionToUserAsync(user.Id, "TELEGRAM");
            }
            if (!string.IsNullOrEmpty(user.Email))
            {
                _logger.LogInformation("Add to user {UserId} email sub", user.Id);
                await userRepository.AddSubscriptionToUserAsync(user.Id, "EMAIL");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in working with user in db while process message from user broker");
            return;
        }

        try
        {
            _logger.LogInformation("Send user {UserId} to telegram broker", user.Id);
            var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramService>();
            await telegramService.SendUserForRegistration(new AuthUser
            {
                Id = user.Id,
                Name = user.Name,
                Telegram = user.Telegram,
                Email = user.Email,
                TelegramChatId = user.TelegramChatId
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error to SendUserForRegistration while process message from user broker");
            return;
        }
    }
}