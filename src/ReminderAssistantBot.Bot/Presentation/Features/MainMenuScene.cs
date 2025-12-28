using ReminderAssistantBot.Bot.Presentation.Common;
using ReminderAssistantBot.Bot.Presentation.UI;
using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Bot.Presentation.Features;

internal sealed class MainMenuScene : IScene
{
    private readonly ISceneRegistry _sceneRegistry;

    public MainMenuScene(ISceneRegistry sceneRegistry) => _sceneRegistry = sceneRegistry;

    public async Task EnterAsync(UpdateContext context, CancellationToken ct)
    {
        BackStackService.Clear(context.Update.UserId);
        await context.Bot.SendTextAsync(context.Update.UserId, CommonUiStrings.Prompts.ChooseAction,
            ParseMode.None, CommonUiKeyboards.MainMenuUiKeyboard.Create(), ct);
    }

    public async Task OnMessageAsync(UpdateContext context, CancellationToken ct)
    {
        await context.Bot.SendTextAsync(context.Update.UserId, CommonUiStrings.Errors.UnknownCmd, ParseMode.None, null, ct);
    }

    public async Task OnCallbackAsync(UpdateContext context, CancellationToken ct)
    {
        string data = context.Update.CallbackData ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(context.Update.CallbackId))
            await context.Bot.AnswerCallbackAsync(context.Update.CallbackId, ct);

        if (string.Equals(data, CommonUiStrings.CallbackData.NavBack, StringComparison.Ordinal))
        {
            await OnBackAsync(context, ct);
            return;
        }

        if (string.Equals(data, CommonUiStrings.CallbackData.NavAddReminder, StringComparison.Ordinal))
        {
            await _sceneRegistry.NavigateForwardAsync(context, SceneKeys.AddReminder, ct);
            return;
        }

        await context.Bot.SendTextAsync(context.Update.UserId, CommonUiStrings.Errors.UnknownCmd, ParseMode.None, null, ct);
    }

    private Task OnBackAsync(UpdateContext context, CancellationToken ct) => EnterAsync(context, ct);
}
