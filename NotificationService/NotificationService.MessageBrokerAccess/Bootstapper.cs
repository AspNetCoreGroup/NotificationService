using Microsoft.Extensions.DependencyInjection;
using NotificationService.Domain.Abstractions.BrokersServices;
using NotificationService.Domain.Abstractions.Repositories;

namespace NotificationService.MessageBrokerAccess;

public static class Bootstapper
{
    public static IServiceCollection AddBrokerAccess(this IServiceCollection services)
    {
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<ITelegramService, TelegramService>();
        services.AddSingleton<IUserService, UserService>();

        return services;
    }
}