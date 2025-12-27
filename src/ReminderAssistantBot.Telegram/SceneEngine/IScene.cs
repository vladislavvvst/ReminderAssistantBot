namespace ReminderAssistantBot.Telegram.SceneEngine;

public interface IScene
{
    string StateKey { get; }
    Task EnterAsync(UpdateContext context, CancellationToken ct);
    Task OnMessageAsync(UpdateContext context, CancellationToken ct);
    Task OnCallbackAsync(UpdateContext context, CancellationToken ct);
    Task OnBackAsync(UpdateContext context, CancellationToken ct);
}
