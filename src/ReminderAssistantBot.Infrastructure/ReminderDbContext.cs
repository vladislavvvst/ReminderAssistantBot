using Microsoft.EntityFrameworkCore;

namespace ReminderAssistantBot.Infrastructure;

internal sealed class ReminderDbContext : DbContext
{
    public DbSet<ReminderEntity> Reminders => Set<ReminderEntity>();

    public ReminderDbContext(DbContextOptions<ReminderDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReminderDbContext).Assembly);
    }
}
