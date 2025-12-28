using ReminderAssistantBot.Telegram.SceneEngine;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

using TgParseMode = Telegram.Bot.Types.Enums.ParseMode;

namespace ReminderAssistantBot.Telegram.Adapters;

internal sealed class TelegramBotClientAdapter : IBotClient
{
    private readonly ITelegramBotClient _bot;

    public TelegramBotClientAdapter(ITelegramBotClient bot) => _bot = bot;

    public Task SendTextAsync(long userId, string text, ParseMode parseMode, BotInlineKeyboard? keyboard, CancellationToken ct)
    {
        InlineKeyboardMarkup? markup = keyboard is null ? null : MapKeyboard(keyboard);
        TgParseMode tgMode = parseMode == ParseMode.Html ? TgParseMode.Html : TgParseMode.None;

        return _bot.SendMessage(userId, text, parseMode: tgMode, replyMarkup: markup, cancellationToken: ct);
    }

    public Task AnswerCallbackAsync(string callbackId, CancellationToken ct) =>
        _bot.AnswerCallbackQuery(callbackId, cancellationToken: ct);

    private static InlineKeyboardMarkup MapKeyboard(BotInlineKeyboard keyboard)
    {
        InlineKeyboardButton[][] rows = keyboard.Rows
            .Select(row => row.Select(b => InlineKeyboardButton.WithCallbackData(b.Text, b.CallbackData)).ToArray())
            .ToArray();

        return new InlineKeyboardMarkup(rows);
    }
}
