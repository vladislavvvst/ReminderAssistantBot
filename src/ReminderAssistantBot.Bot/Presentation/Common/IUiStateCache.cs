namespace ReminderAssistantBot.Bot.Presentation.Common;

internal interface IUiStateCache
{
    Task<int?> GetLastKeyboardMessageIdAsync(long userId);
    Task SetLastKeyboardMessageIdAsync(long userId, int messageId);
    Task ClearLastKeyboardMessageIdAsync(long userId);
}
