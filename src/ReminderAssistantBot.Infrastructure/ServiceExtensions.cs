using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReminderAssistantBot.Application.Abstractions;

namespace ReminderAssistantBot.Infrastructure;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ReminderDbContext>(options =>
        {
            options.UseNpgsql(config.GetConnectionString(nameof(ReminderDbContext)));
        });
        services.AddScoped<IReminderRepository, ReminderRepository>();
        return services;
    }
}
