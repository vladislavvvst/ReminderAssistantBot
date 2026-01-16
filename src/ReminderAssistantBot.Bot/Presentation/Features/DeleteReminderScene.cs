using ReminderAssistantBot.Application.Reminders;
using ReminderAssistantBot.Bot.Presentation.Common;
using ReminderAssistantBot.Bot.Presentation.UI;
using ReminderAssistantBot.Domain;
using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Bot.Presentation.Features;

internal sealed class DeleteReminderScene (IReminderService reminderService, ISceneRegistry sceneRegistry, IUiStateCache uiStateCache) : IScene
{
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);

    public async Task EnterAsync(UpdateContext context, CancellationToken ct)
    {
        await UiKeyboard.ClearPreviousAsync(context, uiStateCache, ct);

        (OperationStatus status, IReadOnlyList<Reminder> activeReminders) = await reminderService.GetActiveAsync(context.Update.UserId, _timeout, ct);
        if (!await TryHandleStatusAsync(context, status, ct))
            return;

        if (activeReminders.Count == 0)
        {
            await UiKeyboard.SendAndTrackAsync(context, uiStateCache, CommonUiStrings.Prompts.ActiveRemindersEmpty,
                ParseMode.None, CommonUiKeyboards.BackUiKeyboard.Create(), ct);
            return;
        }

        await UiKeyboard.SendAndTrackAsync(context, uiStateCache, CommonUiStrings.Prompts.ChooseDeleteReminder,
            ParseMode.Html, CommonUiKeyboards.DeleteReminderKeyboard.Create(activeReminders), ct);
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

        if (string.Equals(data, CommonUiStrings.CallbackData.NavBack, StringComparison.Ordinal))
        {
            await OnBackAsync(context, ct);
            return;
        }

        if (data.StartsWith(CommonUiStrings.CallbackData.NavDeleteRem, StringComparison.Ordinal))
        {
            string idText = data[CommonUiStrings.CallbackData.NavDeleteRem.Length..];
            if (Guid.TryParseExact(idText, "N", out Guid reminderId))
            {
                OperationStatus status = await reminderService.DeleteAsync(userId, reminderId, _timeout,  ct);
                if (await TryHandleStatusAsync(context, status, ct))
                {
                    await context.Bot.SendTextAsync(userId, CommonUiStrings.Prompts.Success, ParseMode.None, null, ct);
                    await EnterAsync(context, ct);
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

    private async Task<bool> TryHandleStatusAsync(UpdateContext context, OperationStatus status, CancellationToken ct)
    {
        if (status is OperationStatus.Success)
            return true;

        string text = status switch
        {
            OperationStatus.Timeout                 => CommonUiStrings.Errors.Timeout,
            OperationStatus.Unavailable             => CommonUiStrings.Errors.Unavailable,
            OperationStatus.NotFound                => CommonUiStrings.Errors.NotFound,
            OperationStatus.Rejected                => CommonUiStrings.Errors.Rejected,
            OperationStatus.ValidationInputFormat   => CommonUiStrings.Errors.ValidationInputFormat,
            OperationStatus.ValidationInputDate     => CommonUiStrings.Errors.ValidationInputDate,
            _                                       => CommonUiStrings.Errors.UnknownError
        };

        await context.Bot.SendTextAsync(context.Update.UserId, text, ParseMode.None, null, ct);
        await OnBackAsync(context, ct);

        return false;
    }
}
