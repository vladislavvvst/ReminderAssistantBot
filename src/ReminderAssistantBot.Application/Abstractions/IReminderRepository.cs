using ReminderAssistantBot.Domain;

namespace ReminderAssistantBot.Application.Abstractions;

public interface IReminderRepository
{
    Task AddAsync(Reminder reminder, CancellationToken ct);
    Task<IReadOnlyList<Reminder>> GetDueAsync(DateTime utcNow, CancellationToken ct);
    Task<IReadOnlyList<Reminder>> GetActiveAsync(long userId, CancellationToken ct);
    Task<bool> DeleteAsync(long userId, Guid reminderId, CancellationToken ct);
    Task UpdateStatusAsync(Guid id, ReminderStatus status, DateTime? sentAtUtc, CancellationToken ct);
}
