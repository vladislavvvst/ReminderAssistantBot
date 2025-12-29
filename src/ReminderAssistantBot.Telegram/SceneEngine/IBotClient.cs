namespace ReminderAssistantBot.Telegram.SceneEngine;

public interface IBotClient
{
    Task SendTextAsync(long userId, string text, ParseMode parseMode, BotInlineKeyboard? keyboard, CancellationToken ct);
    Task<int> SendTextWithIdAsync(long userId, string text, ParseMode parseMode, BotInlineKeyboard? keyboard, CancellationToken ct);
    Task ClearKeyboardAsync(long userId, int messageId, CancellationToken ct);
    Task AnswerCallbackAsync(string callbackId, CancellationToken ct);
}
