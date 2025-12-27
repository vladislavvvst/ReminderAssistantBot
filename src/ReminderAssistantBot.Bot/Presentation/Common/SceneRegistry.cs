using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Bot.Presentation.Common;

internal static class SceneRegistry
{
    private static readonly Dictionary<string, IScene> ByState = [];

    public static IScene GetScene(string stateKey) => Resolve(stateKey);

    public static async Task NavigateBackAsync(UpdateContext context, string fallbackKey, CancellationToken ct)
    {
        long chatId = context.Update.ChatId;
        string stateKey = BackStackService.Pop(chatId) ?? fallbackKey;
        await context.StateCache.SetStateAsync(chatId, stateKey);
        await Resolve(stateKey).EnterAsync(context, ct);
    }

    public static async Task NavigateForwardAsync(UpdateContext context, string nextKey, CancellationToken ct)
    {
        long chatId = context.Update.ChatId;
        string current = await context.StateCache.GetStateAsync(chatId);
        BackStackService.Push(chatId, current);
        await context.StateCache.SetStateAsync(chatId, nextKey);
        await Resolve(nextKey).EnterAsync(context, ct);
    }

    private static void Register(IScene scene) => ByState[scene.StateKey] = scene;

    private static IScene Resolve(string stateKey) =>
        ByState.TryGetValue(stateKey, out IScene? scene) ? scene : ByState[SceneKeys.MainMenu];

    public static void Bootstrap()
    {
        Register(new Features.MainMenu.MainMenuScene());
    }
}
