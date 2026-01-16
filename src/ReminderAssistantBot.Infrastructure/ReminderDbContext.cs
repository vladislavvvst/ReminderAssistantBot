using Microsoft.EntityFrameworkCore;

namespace ReminderAssistantBot.Infrastructure;

internal sealed class ReminderDbContext(DbContextOptions<ReminderDbContext> options) : DbContext(options)
{
    public DbSet<ReminderEntity> Reminders => Set<ReminderEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReminderDbContext).Assembly);
    }
}
