using ReminderAssistantBot.Application.Reminders;
using ReminderAssistantBot.Bot.Presentation.Common;
using ReminderAssistantBot.Bot.Presentation.UI;
using ReminderAssistantBot.Telegram.SceneEngine;
using System.Globalization;

namespace ReminderAssistantBot.Bot.Presentation.Features;

internal sealed class AddReminderScene : IScene
{
    private readonly ICreateReminder _createReminder;
    private readonly ISceneRegistry _sceneRegistry;
    private readonly IUiStateCache _uiStateCache;

    public AddReminderScene(ICreateReminder createReminder, ISceneRegistry sceneRegistry, IUiStateCache uiStateCache)
    {
        _createReminder = createReminder;
        _sceneRegistry = sceneRegistry;
        _uiStateCache = uiStateCache;
    }

    public async Task EnterAsync(UpdateContext context, CancellationToken ct)
    {
        await ShowPromptAsync(context, ct);
    }

    public async Task OnMessageAsync(UpdateContext context, CancellationToken ct)
    {
        await UiKeyboard.ClearPreviousAsync(context, _uiStateCache, ct);

        string input = context.Update.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            await ShowPromptAsync(context, ct);
            return;
        }

        string[] parts = input.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
        {
            await ShowPromptAsync(context, ct);
            return;
        }

        string datePart = parts[0], timePart = parts[1], message = parts[2];

        if (!DateTime.TryParseExact($"{datePart} {timePart}", "dd.MM.yyyy HH:mm",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime localDateTime))
        {
            await ShowPromptAsync(context, ct);
            return;
        }

        // TODO: хард код таймзоны. Нужно спросить пользователя где он находится чтобы корректно парсить время
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
        DateTime dueAtUtc = TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);

        await _createReminder.HandleAsync(context.Update.UserId, message, dueAtUtc, ct);

        await context.Bot.SendTextAsync(context.Update.UserId, CommonUiStrings.Prompts.Success, ParseMode.None, null, ct);
        await _sceneRegistry.NavigateForwardAsync(context, SceneKeys.MainMenu, ct);
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

    private async Task ShowPromptAsync(UpdateContext context, CancellationToken ct)
    {
        await UiKeyboard.SendAndTrackAsync(context, _uiStateCache, CommonUiStrings.Prompts.AddReminder,
            ParseMode.None, CommonUiKeyboards.BackUiKeyboard.Create(), ct);
    }
}
