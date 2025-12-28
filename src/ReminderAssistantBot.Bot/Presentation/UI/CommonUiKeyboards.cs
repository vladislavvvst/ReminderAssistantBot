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
                [ new BotButton(CommonUiStrings.Buttons.AddReminder, CommonUiStrings.CallbackData.NavAddReminder) ]
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
}
