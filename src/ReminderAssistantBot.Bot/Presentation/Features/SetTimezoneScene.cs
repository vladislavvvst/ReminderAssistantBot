using Microsoft.Extensions.Options;
using ReminderAssistantBot.Application.Reminders;
using ReminderAssistantBot.Bot.Options;
using ReminderAssistantBot.Bot.Presentation.Common;
using ReminderAssistantBot.Bot.Presentation.UI;
using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Bot.Presentation.Features;

internal sealed class SetTimezoneScene(IReminderService reminderService, ISceneRegistry sceneRegistry,
    IUiStateCache uiStateCache, IOptions<ReminderOptions> options) : IScene
{
    public async Task EnterAsync(UpdateContext context, CancellationToken ct)
    {
        BackStackService.Clear(context.Update.UserId);
        await UiKeyboard.ClearPreviousAsync(context, uiStateCache, ct);

        (OperationStatus tzStatus, string tzId) = await reminderService.GetTimezoneAsync(context.Update.UserId, options.Value.TimeoutOperation, ct);
        if (!SceneUiUtils.TryHandleStatus(tzStatus, out string errTzMessage))
        {
            await SceneUiUtils.SendErrorAsync(context, errTzMessage, ct);
            await OnBackAsync(context, ct);
            return;
        }

        await UiKeyboard.SendAndTrackAsync(context, uiStateCache,
            $"{CommonUiStrings.Prompts.SetTimezone}\n{CommonUiStrings.Prompts.CurrentTimezone(tzId)}",
            ParseMode.Html, CommonUiKeyboards.SetTimezoneKeyboard.Create(), ct);
    }

    public async Task OnMessageAsync(UpdateContext context, CancellationToken ct)
    {
        await context.Bot.SendTextAsync(context.Update.UserId, CommonUiStrings.Errors.UnknownCmd, ParseMode.None, null, ct);
    }

    public async Task OnCallbackAsync(UpdateContext context, CancellationToken ct)
    {
        string data = context.Update.CallbackData ?? string.Empty;
        long userId = context.Update.UserId;

        if (!string.IsNullOrWhiteSpace(context.Update.CallbackId))
            await context.Bot.AnswerCallbackAsync(context.Update.CallbackId, ct);

        if (data == CommonUiStrings.CallbackData.NavBack)
        {
            await OnBackAsync(context, ct);
            return;
        }

        OperationStatus status = await reminderService.AddOrUpdateTimezoneAsync(userId, data, options.Value.TimeoutOperation, ct);
        if (SceneUiUtils.TryHandleStatus(status, out string errTzMessage))
        {
            await SceneUiUtils.SendSuccessAsync(context, ct);
            await OnBackAsync(context, ct);
        }
        else
        {
            await SceneUiUtils.SendErrorAsync(context, errTzMessage, ct);
            await EnterAsync(context, ct);
        }
    }

    private async Task OnBackAsync(UpdateContext context, CancellationToken ct)
    {
        await UiKeyboard.ClearPreviousAsync(context, uiStateCache, ct);
        await sceneRegistry.NavigateBackAsync(context, SceneKeys.MainMenu, ct);
    }
}
