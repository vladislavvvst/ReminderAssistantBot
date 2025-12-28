namespace ReminderAssistantBot.Telegram.SceneEngine;

public interface IBotClient
{
    Task SendTextAsync(long userId, string text, ParseMode parseMode, BotInlineKeyboard? keyboard, CancellationToken ct);
    Task AnswerCallbackAsync(string callbackId, CancellationToken ct);
}
