using Microsoft.EntityFrameworkCore;
using ReminderAssistantBot.Application.Abstractions;
using ReminderAssistantBot.Domain;

namespace ReminderAssistantBot.Infrastructure;

internal sealed class ReminderRepository : IReminderRepository
{
    private readonly ReminderDbContext _dbContext;

    public ReminderRepository(ReminderDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(Reminder reminder, CancellationToken ct)
    {
        await _dbContext.Reminders.AddAsync(Map(reminder), ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Reminder>> GetDueAsync(DateTime utcNow, CancellationToken ct)
    {
        List<ReminderEntity> entities = await _dbContext.Reminders
            .Where(x => x.Status == ReminderStatus.Pending && x.DueAtUtc <= utcNow)
            .OrderBy(x => x.DueAtUtc)
            .ToListAsync(cancellationToken: ct);

        return entities.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<Reminder>> GetActiveAsync(long userId, CancellationToken ct)
    {
        List<ReminderEntity> entities = await _dbContext.Reminders
            .Where(x => x.Status == ReminderStatus.Pending && x.UserId == userId)
            .OrderBy(x => x.DueAtUtc)
            .ToListAsync(ct);

        return entities.Select(Map).ToList();
    }

    public async Task<bool> DeleteAsync(long userId, Guid reminderId, CancellationToken ct)
    {
        int deleted = await _dbContext.Reminders
            .Where(x => x.Id == reminderId && x.UserId == userId && x.Status == ReminderStatus.Pending)
            .ExecuteDeleteAsync(cancellationToken: ct);

        return deleted > 0;
    }

    public async Task UpdateStatusAsync(Guid id, ReminderStatus status, DateTime? sentAtUtc, CancellationToken ct)
    {
        await _dbContext.Reminders
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, status)
                .SetProperty(x => x.SentAtUtc, sentAtUtc), ct);
    }

    private static ReminderEntity Map(Reminder reminder)
    {
        return new ReminderEntity
        {
            Id           = reminder.Id,
            UserId       = reminder.UserId,
            Message      = reminder.Message,
            DueAtUtc     = reminder.DueAtUtc,
            Status       = reminder.Status,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private static Reminder Map(ReminderEntity reminderEntity)
    {
        return Reminder.Rehydrate
        (
            id:         reminderEntity.Id,
            userId:     reminderEntity.UserId,
            message:    reminderEntity.Message,
            dueAtUtc:   reminderEntity.DueAtUtc,
            status:     reminderEntity.Status
        );
    }
}
