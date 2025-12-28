namespace ReminderAssistantBot.Application.Reminders;

public interface ICreateReminder
{
    Task HandleAsync(long userId, string message, DateTime dueAtUtc, CancellationToken ct);
}
