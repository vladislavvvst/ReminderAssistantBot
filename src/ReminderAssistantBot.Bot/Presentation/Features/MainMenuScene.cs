using ReminderAssistantBot.Bot.Presentation.Common;
using ReminderAssistantBot.Bot.Presentation.UI;
using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Bot.Presentation.Features;

internal sealed class MainMenuScene(ISceneRegistry sceneRegistry, IUiStateCache uiStateCache) : IScene
{
    public async Task EnterAsync(UpdateContext context, CancellationToken ct)
    {
        BackStackService.Clear(context.Update.UserId);
        await UiKeyboard.ClearPreviousAsync(context, uiStateCache, ct);

        await UiKeyboard.SendAndTrackAsync(context, uiStateCache, CommonUiStrings.Prompts.ChooseAction,
            ParseMode.Html, CommonUiKeyboards.MainMenuUiKeyboard.Create(), ct);
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

        switch (data)
        {
            case CommonUiStrings.CallbackData.NavBack:
                await OnBackAsync(context, ct);
                return;

            case CommonUiStrings.CallbackData.NavAddReminder:
                await sceneRegistry.NavigateForwardAsync(context, SceneKeys.AddReminder, ct);
                return;

            case CommonUiStrings.CallbackData.NavDeleteReminder:
                await sceneRegistry.NavigateForwardAsync(context, SceneKeys.DeleteReminder, ct);
                return;

            case CommonUiStrings.CallbackData.NavShowActiveReminders:
                await sceneRegistry.NavigateForwardAsync(context, SceneKeys.ActiveReminders, ct);
                return;

            default:
                await context.Bot.SendTextAsync(context.Update.UserId, CommonUiStrings.Errors.UnknownCmd, ParseMode.None, null, ct);
                return;
        }
    }

    private Task OnBackAsync(UpdateContext context, CancellationToken ct) => EnterAsync(context, ct);
}
