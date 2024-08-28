using Microsoft.Extensions.DependencyInjection;
using NotificationService.Domain.Abstractions.BrokersServices;

namespace NotificationService.MessageBrokerAccess;

public static class Bootstapper
{
    public static IServiceCollection AddBrokerAccess(this IServiceCollection services)
    {
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<ITelegramService, TelegramService>();
        services.AddSingleton<INotificationService, Services.NotificationService>();

        return services;
    }
}