using NodaTime;
using ReminderAssistantBot.Bot.Presentation.Common;
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
        public static BotInlineKeyboard Create(IReadOnlyList<Reminder> reminders, string tzId)
        {
            DateTimeZone timeZone = SceneUiUtils.ResolveTimezone(tzId);

            IReadOnlyList<IReadOnlyList<BotButton>> rows = reminders
                .Select(r =>
                {
                    string local = SceneUiUtils.FormatUtc(r.DueAtUtc, timeZone);
                    string text = $"{local} — {r.Message}";
                    string title = text.Length > 40 ? text[..40] + "…" : text;
                    string data = $"{CommonUiStrings.CallbackData.NavDeleteRem}{r.Id:N}";
                    return (IReadOnlyList<BotButton>)[ new BotButton(title, data) ];
                })
                .ToList();

            rows = rows.Append([ new BotButton(CommonUiStrings.Buttons.Back, CommonUiStrings.CallbackData.NavBack) ]).ToList();
            return new BotInlineKeyboard(rows);
        }
    }

    public static class SetTimezoneKeyboard
    {
        public static BotInlineKeyboard Create()
        {
            IReadOnlyList<IReadOnlyList<BotButton>> rows =
            [
                [ new BotButton("Europe/Moscow", "Europe/Moscow") ],
                [ new BotButton("Europe/Amsterdam", "Europe/Amsterdam") ],
                [ new BotButton(CommonUiStrings.Buttons.Back, CommonUiStrings.CallbackData.NavBack) ]
            ];

            return new BotInlineKeyboard(rows);
        }
    }
}
