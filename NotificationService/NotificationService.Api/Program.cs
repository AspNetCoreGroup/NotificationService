using NotificationService.DataAccess;
using NotificationService.Domain.Abstractions.BrokersServices;
using NotificationService.Domain.Settings;
using NotificationService.MessageBrokerAccess;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;


var services = builder.Services;

services.AddSerilog(lc => lc
    .WriteTo.Console()
    .ReadFrom.Configuration(builder.Configuration));

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddDataAccess(Environment.GetEnvironmentVariable("ConnectionStrings_NotificationService_Postgres") ?? 
                       config.GetConnectionString("PostgresDb") ?? 
                       throw new Exception("No connection string to sql database"));

services.AddBrokerAccess();

// services.Configure<RabbitMqSettings>("UserBroker", options =>
// {
//     options.Host = Environment.GetEnvironmentVariable("RABBITMQ_HOST_BROKER1") ?? "defaultHost1";
//     options.QueueName = config["RabbitMq:Broker1:QueueName"] ?? "defaultQueue1";
// });
//
// services.Configure<RabbitMqSettings>("NotificationBroker", options =>
// {
//     options.Host = Environment.GetEnvironmentVariable("RABBITMQ_HOST_BROKER2") ?? "defaultHost2";
//     options.QueueName = config["RabbitMq:Broker2:QueueName"] ?? "defaultQueue2";
// });

// services
//     .AddOptions<UserRabbitMqConfiguration>()
//     .Bind(new )
//     // .Bind(config.GetSection(nameof(RabbitMqConfiguration)));

var app = builder.Build();

app.Services.Migrate();

using (var scope = app.Services.CreateScope())
{
    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
    var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramService>();
    
    userService.StartListening();
    notificationService.StartListening();
    telegramService.StartListeningForResponses();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
