using Microsoft.Extensions.Options;
using NodaTime;
using ReminderAssistantBot.Application.Reminders;
using ReminderAssistantBot.Bot.Options;
using ReminderAssistantBot.Bot.Presentation.Common;
using ReminderAssistantBot.Bot.Presentation.UI;
using ReminderAssistantBot.Domain;
using ReminderAssistantBot.Telegram.SceneEngine;
using System.Text;

namespace ReminderAssistantBot.Bot.Presentation.Features;

internal sealed class ActiveRemindersScene(IReminderService reminderService, ISceneRegistry sceneRegistry,
    IUiStateCache uiStateCache, IOptions<ReminderOptions> options) : IScene
{
    public async Task EnterAsync(UpdateContext context, CancellationToken ct)
    {
        long userId = context.Update.UserId;
        await UiKeyboard.ClearPreviousAsync(context, uiStateCache, ct);

        (OperationStatus status, IReadOnlyList<Reminder> activeReminders) =
            await reminderService.GetActiveAsync(userId, options.Value.TimeoutOperation, ct);

        if (!SceneUiUtils.TryHandleStatus(status, out string errActiveMessage))
        {
            await SceneUiUtils.SendErrorAsync(context, errActiveMessage, ct);
            await OnBackAsync(context, ct);
            return;
        }

        if (activeReminders.Count is 0)
        {
            await UiKeyboard.SendAndTrackAsync(context, uiStateCache, CommonUiStrings.Prompts.ActiveRemindersEmpty,
                ParseMode.Html, CommonUiKeyboards.BackUiKeyboard.Create(), ct);
            return;
        }

        (OperationStatus tzStatus, string tzId) = await reminderService.GetTimezoneAsync(userId, options.Value.TimeoutOperation, ct);
        if (!SceneUiUtils.TryHandleStatus(tzStatus, out string errTzMessage))
        {
            await SceneUiUtils.SendErrorAsync(context, errTzMessage, ct);
            await OnBackAsync(context, ct);
            return;
        }

        DateTimeZone zone = SceneUiUtils.ResolveTimezone(tzId);

        StringBuilder sb = new();
        sb.AppendLine(CommonUiStrings.Prompts.ActiveReminders);

        int index = 1;
        foreach (Reminder reminder in activeReminders)
        {
            string local = SceneUiUtils.FormatUtc(reminder.DueAtUtc, zone);
            sb.AppendLine($"{index}) {local} - {reminder.Message}");
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
}
