namespace ReminderAssistantBot.Bot.Presentation.UI;

internal static class CommonUiStrings
{
    internal static class CallbackData
    {
        public const string NavBack = "nav:back";
        public const string NavAddReminder = "rem:add";
    }

    internal static class Commands
    {
        public const string Start = "/start";
        public const string Menu = "/menu";
        public const string About = "/about";
        public const string Cancel = "/cancel";
    }

    internal static class Buttons
    {
        public const string BotStart = "Запустить бота";
        public const string BotMenu = "Открыть меню";
        public const string BotAbout = "О боте";

        public const string Back = "⬅️ Назад";
        public const string AddReminder = "➕ Добавить напоминание";
    }

    internal static class Prompts
    {
        public const string ChooseAction = "Выберите действие:";
        public const string AddReminder = "⏰ Установите напоминание\nФормат ввода:\n01.01.2026 00:00 Новый Год";
        public const string AboutBot =
            "<b>Reminder Assistant Bot - это ассистент напоминаний. " +
            "Он помогает создавать, просматривать и управлять задачами, используя диалоговые сцены и удобную навигацию</b>";
        public const string Success = "✔ Успех";
    }

    internal static class Errors
    {
        public const string UnknownCmd = "🤷‍♂️ Неизвестная команда";
    }
}
