using ReminderAssistantBot.Application.Reminders;
using ReminderAssistantBot.Bot.Presentation.Common;
using ReminderAssistantBot.Bot.Presentation.UI;
using ReminderAssistantBot.Domain;
using ReminderAssistantBot.Telegram.SceneEngine;
using System.Text;

namespace ReminderAssistantBot.Bot.Presentation.Features;

internal sealed class ActiveRemindersScene : IScene
{
    private readonly IReminderQueries _reminderQueries;
    private readonly ISceneRegistry _sceneRegistry;
    private readonly IUiStateCache _uiStateCache;

    public ActiveRemindersScene(IReminderQueries reminderQueries, ISceneRegistry sceneRegistry, IUiStateCache uiStateCache)
    {
        _reminderQueries = reminderQueries;
        _sceneRegistry = sceneRegistry;
        _uiStateCache = uiStateCache;
    }

    public async Task EnterAsync(UpdateContext context, CancellationToken ct)
    {
        long userId = context.Update.UserId;
        await UiKeyboard.ClearPreviousAsync(context, _uiStateCache, ct);

        IReadOnlyList<Reminder> activeReminders = await _reminderQueries.GetActiveAsync(userId, ct);

        if (activeReminders.Count == 0)
        {
            await UiKeyboard.SendAndTrackAsync(context, _uiStateCache, CommonUiStrings.Prompts.ActiveRemindersEmpty,
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

        await UiKeyboard.SendAndTrackAsync(context, _uiStateCache, sb.ToString(),
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
        await UiKeyboard.ClearPreviousAsync(context, _uiStateCache, ct);
        await _sceneRegistry.NavigateBackAsync(context, SceneKeys.MainMenu, ct);
    }
}
