﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.DataAccess.Repositories;
using NotificationService.Domain.Abstractions.Repositories;

namespace NotificationService.DataAccess;

public static class Bootstapper
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
    {
        services
            .AddDbContext<DataContext>(x =>
            {
                x.UseNpgsql(connectionString);
            })
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IMessageRepository, MessageRepository>();

        return services;
    }
}