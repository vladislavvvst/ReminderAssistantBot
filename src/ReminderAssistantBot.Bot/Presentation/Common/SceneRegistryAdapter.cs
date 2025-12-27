using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Bot.Presentation.Common;

internal sealed class SceneRegistryAdapter : ISceneRegistry
{
    public IScene GetScene(string stateKey) => SceneRegistry.GetScene(stateKey);

    public Task NavigateBackAsync(UpdateContext context, string fallbackKey, CancellationToken ct) =>
        SceneRegistry.NavigateBackAsync(context, fallbackKey, ct);

    public Task NavigateForwardAsync(UpdateContext context, string nextKey, CancellationToken ct) =>
        SceneRegistry.NavigateForwardAsync(context, nextKey, ct);
}
