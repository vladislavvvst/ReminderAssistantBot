using ReminderAssistantBot.Application.Reminders;
using ReminderAssistantBot.Bot.Presentation.Common;
using ReminderAssistantBot.Bot.Presentation.UI;
using ReminderAssistantBot.Domain;
using ReminderAssistantBot.Telegram.SceneEngine;
using System.Text;

namespace ReminderAssistantBot.Bot.Presentation.Features;

internal sealed class ActiveRemindersScene(IReminderService reminderService, ISceneRegistry sceneRegistry, IUiStateCache uiStateCache) : IScene
{
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);

    public async Task EnterAsync(UpdateContext context, CancellationToken ct)
    {
        long userId = context.Update.UserId;
        await UiKeyboard.ClearPreviousAsync(context, uiStateCache, ct);

        (OperationStatus status, IReadOnlyList<Reminder> activeReminders) = await reminderService.GetActiveAsync(userId, _timeout, ct);
        if (!await TryHandleStatusAsync(context, status, ct))
            return;

        if (activeReminders.Count is 0)
        {
            await UiKeyboard.SendAndTrackAsync(context, uiStateCache, CommonUiStrings.Prompts.ActiveRemindersEmpty,
                ParseMode.None, CommonUiKeyboards.BackUiKeyboard.Create(), ct);
            return;
        }

        // TODO: хард код таймзоны. Нужно спросить пользователя где он находится чтобы корректно парсить время
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");

        StringBuilder sb = new();
        sb.AppendLine(CommonUiStrings.Prompts.ActiveReminders);

        int index = 1;
        foreach (Reminder reminder in activeReminders)
        {
            DateTime local = TimeZoneInfo.ConvertTimeFromUtc(reminder.DueAtUtc, timeZone);
            sb.AppendLine($"{index}) {local:dd.MM.yyyy HH:mm} - {reminder.Message}");
            index++;
        }

        await UiKeyboard.SendAndTrackAsync(context, uiStateCache, sb.ToString(),
            ParseMode.Html, CommonUiKeyboards.BackUiKeyboard.Create(), ct);
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

        await context.Bot.SendTextAsync(context.Update.UserId, CommonUiStrings.Errors.UnknownCmd, ParseMode.None, null, ct);
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
