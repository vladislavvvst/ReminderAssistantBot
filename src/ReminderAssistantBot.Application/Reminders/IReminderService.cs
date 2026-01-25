using ReminderAssistantBot.Domain;

namespace ReminderAssistantBot.Application.Reminders;

public interface IReminderService
{
    Task<OperationStatus> CreateAsync(long userId, string message, DateTime dueAtUtc, TimeSpan timeout, CancellationToken ct);
    Task<OperationStatus> DeleteAsync(long userId, Guid reminderId, TimeSpan timeout, CancellationToken ct);
    Task<(OperationStatus, IReadOnlyList<Reminder>)> GetActiveAsync(long userId, TimeSpan timeout, CancellationToken ct);
    Task<OperationStatus> AddOrUpdateTimezoneAsync(long userId, string timezoneKey, TimeSpan timeout, CancellationToken ct);
    Task<(OperationStatus, string)> GetTimezoneAsync(long userId, TimeSpan timeout, CancellationToken ct);
}
