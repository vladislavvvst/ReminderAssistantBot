using Microsoft.Extensions.Options;
using ReminderAssistantBot.Application.Reminders;
using ReminderAssistantBot.Bot.Options;
using ReminderAssistantBot.Bot.Presentation.Common;
using ReminderAssistantBot.Bot.Presentation.UI;
using ReminderAssistantBot.Telegram.SceneEngine;
using System.Globalization;

namespace ReminderAssistantBot.Bot.Presentation.Features;

internal sealed class AddReminderScene(IReminderService reminderService, ISceneRegistry sceneRegistry,
    IUiStateCache uiStateCache, IOptions<ReminderOptions> options) : IScene
{
    public async Task EnterAsync(UpdateContext context, CancellationToken ct)
    {
        await UiKeyboard.SendAndTrackAsync(context, uiStateCache, CommonUiStrings.Prompts.AddReminder,
            ParseMode.None, CommonUiKeyboards.BackUiKeyboard.Create(), ct);
    }

    public async Task OnMessageAsync(UpdateContext context, CancellationToken ct)
    {
        await UiKeyboard.ClearPreviousAsync(context, uiStateCache, ct);

        string input = context.Update.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            await EnterAsync(context, ct);
            return;
        }

        string[] parts = input.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
        {
            await ShowValidationAsync(context, CommonUiStrings.Errors.ValidationInputFormat, ct);
            return;
        }

        string datePart = parts[0], timePart = parts[1], message = parts[2];

        if (!DateTime.TryParseExact($"{datePart} {timePart}", "dd.MM.yyyy HH:mm",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime localDateTime))
        {
            await ShowValidationAsync(context, CommonUiStrings.Errors.ValidationInputFormat, ct);
            return;
        }

        // TODO: хард код таймзоны. Нужно спросить пользователя где он находится чтобы корректно парсить время
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
        DateTime dueAtUtc = TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);

        if (dueAtUtc < DateTime.UtcNow)
        {
            await ShowValidationAsync(context, CommonUiStrings.Errors.ValidationInputDate, ct);
            return;
        }

        OperationStatus status = await reminderService.CreateAsync(context.Update.UserId, message, dueAtUtc, options.Value.TimeoutOperation, ct);
        await HandleStatusAsync(context, status, ct);
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

    private async Task ShowValidationAsync(UpdateContext context, string text, CancellationToken ct)
    {
        await context.Bot.SendTextAsync(context.Update.UserId, text, ParseMode.None, null, ct);
        await EnterAsync(context, ct);
    }

    private async Task HandleStatusAsync(UpdateContext context, OperationStatus status, CancellationToken ct)
    {
        if (status is OperationStatus.Success)
        {
            await context.Bot.SendTextAsync(context.Update.UserId, CommonUiStrings.Prompts.Success, ParseMode.None, null, ct);
            await EnterAsync(context, ct);
            return;
        }

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
    }
}
