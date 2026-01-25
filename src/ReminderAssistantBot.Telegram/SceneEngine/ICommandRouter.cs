namespace ReminderAssistantBot.Telegram.SceneEngine;

public interface ICommandRouter
{
    Task<bool> TryHandleAsync(UpdateContext context, CancellationToken ct);
}
