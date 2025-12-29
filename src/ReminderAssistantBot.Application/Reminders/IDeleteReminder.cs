namespace ReminderAssistantBot.Application.Reminders;

public interface IDeleteReminder
{
    Task<bool> HandleAsync(long userId, Guid reminderId, CancellationToken ct);
}
