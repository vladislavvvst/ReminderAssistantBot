namespace ReminderAssistantBot.Telegram.SceneEngine;

public interface ISceneRegistry
{
    IScene GetScene(string stateKey);
    Task NavigateBackAsync(UpdateContext context, string fallbackKey, CancellationToken ct);
    Task NavigateForwardAsync(UpdateContext context, string nextKey, CancellationToken ct);
}
