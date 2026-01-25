namespace ReminderAssistantBot.Bot.Presentation.UI;

internal static class CommonUiStrings
{
    internal static class CallbackData
    {
        public const string NavBack = "nav:back";
        public const string NavAddReminder = "rem:add";
        public const string NavShowActiveReminders = "rem:showActiveReminders";
        public const string NavDeleteReminder = "rem:deleteReminder";
        public const string NavDeleteRem = "rem:del:";
    }

    internal static class Commands
    {
        public const string Start = "/start";
        public const string Menu = "/menu";
        public const string About = "/about";
        public const string Cancel = "/cancel";
        public const string Timezone = "/timezone";
    }

    internal static class Buttons
    {
        public const string BotStart = "Запустить бота";
        public const string BotMenu = "Открыть меню";
        public const string BotAbout = "О боте";
        public const string BotTimezone = "Установить таймзону";

        public const string Back = "⬅️ Назад";
        public const string AddReminder = "➕ Добавить напоминание";
        public const string DeleteReminder = "🗑️ Удалить напоминание";
        public const string ShowActiveReminders = "📝 Показать активные";
    }

    internal static class Prompts
    {
        public const string ChooseAction = "<b>Выберите действие:</b>";
        public const string ActiveReminders = "<b>Активные:</b>";
        public const string ActiveRemindersEmpty = "<b>Активных напоминаний нет</b>";
        public const string ChooseDeleteReminder = "<b>Выберите напоминание для удаления:</b>";
        public const string AddReminder = "⏰ Установите напоминание\nФормат ввода:\n01.01.2026 00:00 Новый Год";
        public const string Success = "<b>✔ Успех</b>";
        public const string SetTimezone = "<b>Выберите таймзону:</b>";
        public const string AboutBot =
            "<b>Reminder Assistant Bot</b> — это ассистент напоминаний. " +
            "Он помогает создавать, просматривать и управлять задачами, с помощью диалоговых сцен и удобной навигации.\n" +
            "❗Бот предназначен для бытовых напоминаний. " +
            "Он хранит только Ваш chatId и текст напоминаний — исключительно для работы функционала. " +
            "Пожалуйста, не добавляйте в напоминания пароли, данные банковских карт, паспортные данные, медицинскую или другую чувствительную информацию.\n" +
            "По запросу пользователя данные могут быть удалены через кнопку «Удалить напоминание».";

        public static string CurrentTimezone(string tz) => $"Текущая: 🌍{tz}";
    }

    internal static class Errors
    {
        public const string UnknownCmd = "🤷‍♂️ Неизвестная команда";
        public const string NotFound = "🤷‍♂️ Такого нету";
        public const string Timeout = "🤷‍♂️ Превышено время ожидания";
        public const string Unavailable = "🤷‍♂️ Сервис недоступен";
        public const string ValidationInputFormat = "🤷‍♂️ Неверный ввод (см. формат ввода)";
        public const string ValidationInputDate = "🤷‍♂️ Укажите будущее время";
        public const string UnknownError = "🤷‍♂️ Неизвестная ошибка";
        public const string Rejected = "🤷‍♂️ Отклонено";
    }
}
