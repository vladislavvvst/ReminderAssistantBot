using Microsoft.EntityFrameworkCore;
using ReminderAssistantBot.Infrastructure.Reminders;
using ReminderAssistantBot.Infrastructure.UserSettings;

namespace ReminderAssistantBot.Infrastructure;

internal sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<ReminderEntity> Reminders => Set<ReminderEntity>();
    public DbSet<UserSettingsEntity> UserSettings => Set<UserSettingsEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
