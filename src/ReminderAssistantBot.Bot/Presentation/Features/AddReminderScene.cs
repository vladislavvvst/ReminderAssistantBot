using Microsoft.Extensions.Options;
using NodaTime;
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

        long userId = context.Update.UserId;
        string input = context.Update.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            await ShowValidationAsync(context, CommonUiStrings.Errors.ValidationInputFormat, ct);
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

        (OperationStatus tzStatus, string tzId) = await reminderService.GetTimezoneAsync(userId, options.Value.TimeoutOperation, ct);
        if (!SceneUiUtils.TryResolveTimezone(tzStatus, tzId, out DateTimeZone zone, out string errTzMessage))
        {
            await SceneUiUtils.SendErrorAsync(context, errTzMessage, ct);
            await OnBackAsync(context, ct);
            return;
        }

        DateTime dueAtUtc = SceneUiUtils.ToUtc(localDateTime, zone);

        if (dueAtUtc < DateTime.UtcNow)
        {
            await ShowValidationAsync(context, CommonUiStrings.Errors.ValidationInputDate, ct);
            return;
        }

        OperationStatus status = await reminderService.CreateAsync(context.Update.UserId, message, dueAtUtc, options.Value.TimeoutOperation, ct);
        if (SceneUiUtils.TryHandleStatus(status, out string errCreateMessage))
        {
            await SceneUiUtils.SendSuccessAsync(context, ct);
            await EnterAsync(context, ct);
        }
        else
        {
            await SceneUiUtils.SendErrorAsync(context, errCreateMessage, ct);
            await OnBackAsync(context, ct);
        }
    }

    public async Task OnCallbackAsync(UpdateContext context, CancellationToken ct)
    {
        string data = context.Update.CallbackData ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(context.Update.CallbackId))
            await context.Bot.AnswerCallbackAsync(context.Update.CallbackId, ct);

        if (data == CommonUiStrings.CallbackData.NavBack)
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
}
