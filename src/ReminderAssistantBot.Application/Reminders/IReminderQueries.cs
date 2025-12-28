using ReminderAssistantBot.Domain;

namespace ReminderAssistantBot.Application.Reminders;

public interface IReminderQueries
{
    Task<IReadOnlyList<Reminder>> GetActiveAsync(long userId, CancellationToken ct);
}
