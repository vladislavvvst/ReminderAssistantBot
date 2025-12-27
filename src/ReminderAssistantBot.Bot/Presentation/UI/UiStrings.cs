namespace ReminderAssistantBot.Bot.Presentation.UI;

internal static class UiStrings
{
    internal static class CallbackData
    {
        public const string NavBack = "nav:back";
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
    }

    internal static class Prompts
    {
        public const string AboutBot =
            "<b>Reminder Assistant Bot - это ассистент напоминаний. " +
            "Он помогает создавать, просматривать и управлять задачами, используя диалоговые сцены и удобную навигацию</b>";
    }

    internal static class Errors
    {
        public const string UnknownCmd = "🤷‍♂️ Неизвестная команда";
    }
}
