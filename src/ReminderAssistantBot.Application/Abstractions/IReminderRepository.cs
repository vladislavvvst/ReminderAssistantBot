using ReminderAssistantBot.Domain;

namespace ReminderAssistantBot.Application.Abstractions;

public interface IReminderRepository
{
    Task AddAsync(Reminder reminder, CancellationToken ct);
    Task<IReadOnlyList<Reminder>> GetDueAsync(DateTime utcNow, CancellationToken ct);
    Task<IReadOnlyList<Reminder>> GetActiveAsync(long userId, CancellationToken ct);
    Task<int> DeleteAsync(long userId, Guid reminderId, CancellationToken ct);
    Task<int> UpdateStatusAsync(Guid id, ReminderStatus status, DateTime? sentAtUtc, CancellationToken ct);
    Task AddOrUpdateTimezoneAsync(long userId, string timezoneKey, CancellationToken ct);
    Task<string> GetTimezoneAsync(long userId, TimeSpan timeout, CancellationToken ct);
}
