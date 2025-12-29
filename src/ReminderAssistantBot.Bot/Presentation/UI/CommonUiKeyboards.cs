using ReminderAssistantBot.Domain;
using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Bot.Presentation.UI;

internal static class CommonUiKeyboards
{
    public static class MainMenuUiKeyboard
    {
        public static BotInlineKeyboard Create()
        {
            IReadOnlyList<IReadOnlyList<BotButton>> rows =
            [
                [ new BotButton(CommonUiStrings.Buttons.AddReminder, CommonUiStrings.CallbackData.NavAddReminder) ],
                [ new BotButton(CommonUiStrings.Buttons.DeleteReminder, CommonUiStrings.CallbackData.NavDeleteReminder) ],
                [ new BotButton(CommonUiStrings.Buttons.ShowActiveReminders, CommonUiStrings.CallbackData.NavShowActiveReminders) ]
            ];

            return new BotInlineKeyboard(rows);
        }
    }

    public static class BackUiKeyboard
    {
        public static BotInlineKeyboard Create()
        {
            IReadOnlyList<IReadOnlyList<BotButton>> rows =
            [
                [ new BotButton(CommonUiStrings.Buttons.Back, CommonUiStrings.CallbackData.NavBack) ]
            ];

            return new BotInlineKeyboard(rows);
        }
    }

    public static class DeleteReminderKeyboard
    {
        public static BotInlineKeyboard Create(IReadOnlyList<Reminder> reminders)
        {
            // TODO: хард код таймзоны. Нужно спросить пользователя где он находится чтобы корректно парсить время
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");

            IReadOnlyList<IReadOnlyList<BotButton>> rows = reminders
                .Select(r =>
                {
                    DateTime local = TimeZoneInfo.ConvertTimeFromUtc(r.DueAtUtc, timeZone);
                    string text = $"{local:dd.MM.yyyy HH:mm} — {r.Message}";
                    string title = text.Length > 40 ? text[..40] + "…" : text;
                    string data = $"{CommonUiStrings.CallbackData.NavDeleteRem}{r.Id:N}";
                    return (IReadOnlyList<BotButton>)[ new BotButton(title, data) ];
                })
                .ToList();

            rows = rows.Append([ new BotButton(CommonUiStrings.Buttons.Back, CommonUiStrings.CallbackData.NavBack) ]).ToList();
            return new BotInlineKeyboard(rows);
        }
    }
}
