using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReminderAssistantBot.Infrastructure.Reminders;

namespace ReminderAssistantBot.Infrastructure;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        ApplicationDbContext applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await applicationDbContext.Database.MigrateAsync();
    }
}
