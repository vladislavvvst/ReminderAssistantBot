using Microsoft.Extensions.Options;
using ReminderAssistantBot.Application.Reminders;
using ReminderAssistantBot.Bot.Options;
using ReminderAssistantBot.Bot.Presentation.Common;
using ReminderAssistantBot.Bot.Presentation.UI;
using ReminderAssistantBot.Domain;
using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Bot.Presentation.Features;

internal sealed class DeleteReminderScene (IReminderService reminderService, ISceneRegistry sceneRegistry,
    IUiStateCache uiStateCache, IOptions<ReminderOptions> options) : IScene
{
    public async Task EnterAsync(UpdateContext context, CancellationToken ct)
    {
        await UiKeyboard.ClearPreviousAsync(context, uiStateCache, ct);

        (OperationStatus status, IReadOnlyList<Reminder> activeReminders) = await reminderService.GetActiveAsync(context.Update.UserId, options.Value.TimeoutOperation, ct);
        if (!SceneUiUtils.TryHandleStatus(status, out string errActiveMsg))
        {
            await SceneUiUtils.SendErrorAsync(context, errActiveMsg, ct);
            await OnBackAsync(context, ct);
            return;
        }

        if (activeReminders.Count == 0)
        {
            await UiKeyboard.SendAndTrackAsync(context, uiStateCache, CommonUiStrings.Prompts.ActiveRemindersEmpty,
                ParseMode.Html, CommonUiKeyboards.BackUiKeyboard.Create(), ct);
            return;
        }

        (OperationStatus tzStatus, string tzId) = await reminderService.GetTimezoneAsync(context.Update.UserId, options.Value.TimeoutOperation, ct);
        if (!SceneUiUtils.TryHandleStatus(tzStatus, out string errTimezoneMsg))
        {
            await SceneUiUtils.SendErrorAsync(context, errTimezoneMsg, ct);
            await OnBackAsync(context, ct);
            return;
        }

        await UiKeyboard.SendAndTrackAsync(context, uiStateCache, CommonUiStrings.Prompts.ChooseDeleteReminder,
            ParseMode.Html, CommonUiKeyboards.DeleteReminderKeyboard.Create(activeReminders, tzId), ct);
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

        if (data.StartsWith(CommonUiStrings.CallbackData.NavDeleteRem, StringComparison.Ordinal))
        {
            string idText = data[CommonUiStrings.CallbackData.NavDeleteRem.Length..];
            if (Guid.TryParseExact(idText, "N", out Guid reminderId))
            {
                OperationStatus status = await reminderService.DeleteAsync(userId, reminderId, options.Value.TimeoutOperation, ct);
                if (SceneUiUtils.TryHandleStatus(status, out string errorMessage))
                {
                    await SceneUiUtils.SendSuccessAsync(context, ct);
                    await EnterAsync(context, ct);
                }
                else
                {
                    await SceneUiUtils.SendErrorAsync(context, errorMessage, ct);
                    await OnBackAsync(context, ct);
                }

                return;
            }
        }

        await context.Bot.SendTextAsync(userId, CommonUiStrings.Errors.UnknownCmd, ParseMode.None, null, ct);
    }

    private async Task OnBackAsync(UpdateContext context, CancellationToken ct)
    {
        await UiKeyboard.ClearPreviousAsync(context, uiStateCache, ct);
        await sceneRegistry.NavigateBackAsync(context, SceneKeys.MainMenu, ct);
    }
}
