namespace ReminderAssistantBot.Telegram.SceneEngine;

internal static class SceneRouter
{
    public static async Task RouteAsync(UpdateContext context, ISceneRegistry registry, CancellationToken ct)
    {
        string stateKey = await context.StateCache.GetStateAsync(context.Update.ChatId);
        IScene scene = registry.GetScene(stateKey);

        switch (context.Update.Kind)
        {
            case UpdateKind.Callback:
                await scene.OnCallbackAsync(context, ct);
                return;
            case UpdateKind.Message:
                await scene.OnMessageAsync(context, ct);
                return;
            default:
                await scene.EnterAsync(context, ct);
                return;
        }
    }
}
