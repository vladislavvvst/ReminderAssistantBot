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

        if (string.Equals(cmd, UiStrings.Commands.Start, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(cmd, UiStrings.Commands.Menu, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(cmd, UiStrings.Commands.Cancel, StringComparison.OrdinalIgnoreCase))
        {
            await _sceneRegistry.NavigateForwardAsync(context, SceneKeys.MainMenu, ct);
            return true;
        }

        if (string.Equals(cmd, UiStrings.Commands.About, StringComparison.OrdinalIgnoreCase))
        {
            await context.Bot.SendTextAsync(context.Update.ChatId, UiStrings.Prompts.AboutBot, ParseMode.Html, null, ct);
            return true;
        }

        return false;
    }
}
