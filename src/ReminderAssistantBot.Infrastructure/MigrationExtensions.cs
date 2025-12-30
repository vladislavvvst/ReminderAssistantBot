using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ReminderAssistantBot.Infrastructure;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        ReminderDbContext db = scope.ServiceProvider.GetRequiredService<ReminderDbContext>();
        await db.Database.MigrateAsync();
    }
}
