using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace NotificationService.DataAccess;

public static class Bootstapper
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
    {
        return services
            .AddDbContext<DataContext>(x =>
            {
                x.UseNpgsql(connectionString);
            });
    }
}