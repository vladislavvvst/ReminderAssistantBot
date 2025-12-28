namespace ReminderAssistantBot.Application.Reminders;

public interface IReminderSender
{
    Task SendAsync(long userId, string message, CancellationToken ct);
}
