namespace ReminderAssistantBot.Telegram.SceneEngine;

public interface IScene
{
    Task EnterAsync(UpdateContext context, CancellationToken ct);
    Task OnMessageAsync(UpdateContext context, CancellationToken ct);
    Task OnCallbackAsync(UpdateContext context, CancellationToken ct);
}
