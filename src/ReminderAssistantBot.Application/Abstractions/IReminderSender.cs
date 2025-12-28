namespace ReminderAssistantBot.Application.Abstractions;

public interface IReminderSender
{
    Task SendAsync(long userId, string message, CancellationToken ct);
}
