using ReminderAssistantBot.Application.Reminders;
using ReminderAssistantBot.Bot.Presentation.Common;
using ReminderAssistantBot.Bot.Presentation.UI;
using ReminderAssistantBot.Domain;
using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Bot.Presentation.Features;

internal sealed class DeleteReminderScene : IScene
{
    private readonly IReminderQueries _reminderQueries;
    private readonly IDeleteReminder _deleteReminder;
    private readonly ISceneRegistry _sceneRegistry;

    public DeleteReminderScene(IReminderQueries reminderQueries, IDeleteReminder deleteReminder, ISceneRegistry sceneRegistry)
    {
        _reminderQueries = reminderQueries;
        _deleteReminder = deleteReminder;
        _sceneRegistry = sceneRegistry;
    }

    public async Task EnterAsync(UpdateContext context, CancellationToken ct)
    {
        IReadOnlyList<Reminder> activeReminders = await _reminderQueries.GetActiveAsync(context.Update.UserId, ct);

        if (activeReminders.Count == 0)
        {
            await context.Bot.SendTextAsync(context.Update.UserId, CommonUiStrings.Prompts.ActiveRemindersEmpty,
                ParseMode.None, CommonUiKeyboards.BackUiKeyboard.Create(), ct);
            return;
        }

        await context.Bot.SendTextAsync(context.Update.UserId, CommonUiStrings.Prompts.ChooseDeleteReminder,
            ParseMode.None, CommonUiKeyboards.DeleteReminderKeyboard.Create(activeReminders), ct);
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
                bool deleted = await _deleteReminder.HandleAsync(userId, reminderId, ct);

                await context.Bot.SendTextAsync(userId, deleted
                    ? CommonUiStrings.Prompts.Success
                    : CommonUiStrings.Errors.NotFound, ParseMode.None, null, ct);

                await OnBackAsync(context, ct);
                return;
            }
        }

        await context.Bot.SendTextAsync(userId, CommonUiStrings.Errors.UnknownCmd, ParseMode.None, null, ct);
    }

    private Task OnBackAsync(UpdateContext context, CancellationToken ct) =>
        _sceneRegistry.NavigateBackAsync(context, SceneKeys.MainMenu, ct);
}
