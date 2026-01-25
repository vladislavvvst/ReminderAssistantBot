namespace ReminderAssistantBot.Telegram.SceneEngine;

public sealed class BotInlineKeyboard
{
    public IReadOnlyList<IReadOnlyList<BotButton>> Rows { get; }
    public BotInlineKeyboard(IReadOnlyList<IReadOnlyList<BotButton>> rows) => Rows = rows;
}
