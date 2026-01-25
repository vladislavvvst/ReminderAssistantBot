using Microsoft.Extensions.DependencyInjection;
using ReminderAssistantBot.Bot.Presentation.Features;
using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Bot.Presentation.Common;

internal sealed class SceneRegistry : ISceneRegistry
{
    private readonly IServiceProvider _provider;
    private readonly IReadOnlyDictionary<string, Type> _sceneTypes;

    public SceneRegistry(IServiceProvider provider)
    {
        _provider = provider;
        _sceneTypes = new Dictionary<string, Type>
        {
            [SceneKeys.MainMenu]        = typeof(MainMenuScene),
            [SceneKeys.AddReminder]     = typeof(AddReminderScene),
            [SceneKeys.ActiveReminders] = typeof(ActiveRemindersScene),
            [SceneKeys.DeleteReminder]  = typeof(DeleteReminderScene),
            [SceneKeys.SetTimezone]     = typeof(SetTimezoneScene)
        };
    }

    public IScene GetScene(string stateKey)
    {
        Type sceneType = _sceneTypes.TryGetValue(stateKey, out Type? type)
            ? type
            : _sceneTypes[SceneKeys.MainMenu];

        return (IScene)_provider.GetRequiredService(sceneType);
    }

    public async Task NavigateBackAsync(UpdateContext context, string fallbackKey, CancellationToken ct)
    {
        long userId = context.Update.UserId;
        string stateKey = BackStackService.Pop(userId) ?? fallbackKey;
        await context.StateCache.SetStateAsync(userId, stateKey);
        await GetScene(stateKey).EnterAsync(context, ct);
    }

    public async Task NavigateForwardAsync(UpdateContext context, string nextKey, CancellationToken ct)
    {
        long userId = context.Update.UserId;
        string current = await context.StateCache.GetStateAsync(userId);
        BackStackService.Push(userId, current);
        await context.StateCache.SetStateAsync(userId, nextKey);
        await GetScene(nextKey).EnterAsync(context, ct);
    }
}
