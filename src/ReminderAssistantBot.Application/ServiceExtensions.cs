using Microsoft.Extensions.DependencyInjection;
using ReminderAssistantBot.Application.Reminders;

namespace ReminderAssistantBot.Application;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IReminderService, ReminderService>();
        services.AddScoped<DispatchDueReminders>();
        return services;
    }
}
