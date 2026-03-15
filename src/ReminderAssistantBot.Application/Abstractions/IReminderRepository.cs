using ReminderAssistantBot.Domain;

namespace ReminderAssistantBot.Application.Abstractions;

public interface IReminderRepository
{
    Task AddAsync(Reminder reminder, CancellationToken ct);
    Task<IReadOnlyList<Reminder>> GetActiveAsync(long userId, CancellationToken ct);
    Task<int> DeleteAsync(long userId, Guid reminderId, CancellationToken ct);
    Task AddOrUpdateTimezoneAsync(long userId, string timezoneKey, CancellationToken ct);
    Task<string> GetTimezoneAsync(long userId, CancellationToken ct);

    Task<IReadOnlyList<ClaimedReminder>> ClaimDueAsync(DateTime utcNow, TimeSpan leaseDuration, int batchSize, CancellationToken ct);
    Task<int> MarkSentAsync(Guid id, Guid leaseToken, DateTime sentAtUtc, CancellationToken ct);
}
