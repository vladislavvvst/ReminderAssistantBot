using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReminderAssistantBot.Application.Abstractions;

namespace ReminderAssistantBot.Infrastructure;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(config.GetConnectionString(nameof(ApplicationDbContext)));
        });
        services.AddScoped<IReminderRepository, ReminderRepository>();
        return services;
    }
}
