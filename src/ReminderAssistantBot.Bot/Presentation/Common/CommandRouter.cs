using ReminderAssistantBot.Bot.Presentation.UI;
using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Bot.Presentation.Common;

internal sealed class CommandRouter : ICommandRouter
{
    private readonly ISceneRegistry _sceneRegistry;

    public CommandRouter(ISceneRegistry sceneRegistry) => _sceneRegistry = sceneRegistry;

    public async Task<bool> TryHandleAsync(UpdateContext context, CancellationToken ct)
    {
        if (context.Update.Kind != UpdateKind.Message || string.IsNullOrWhiteSpace(context.Update.Text))
            return false;

        string text = context.Update.Text!;

        if (!text.StartsWith('/'))
            return false;

        string cmd = text.Split(' ', 2)[0];

        switch (cmd)
        {
            case CommonUiStrings.Commands.Start or CommonUiStrings.Commands.Menu or CommonUiStrings.Commands.Cancel:
                await _sceneRegistry.NavigateForwardAsync(context, SceneKeys.MainMenu, ct);
                return true;

            case CommonUiStrings.Commands.About:
                await context.Bot.SendTextAsync(context.Update.UserId, CommonUiStrings.Prompts.AboutBot, ParseMode.Html, null, ct);
                return true;

            case CommonUiStrings.Commands.Timezone:
                await _sceneRegistry.NavigateForwardAsync(context, SceneKeys.SetTimezone, ct);
                return true;

            default:
                return false;
        }
    }
}
