namespace ReminderAssistantBot.Telegram.SceneEngine;

public sealed class BotInlineKeyboard
{
    public BotInlineKeyboard(IReadOnlyList<IReadOnlyList<BotButton>> rows) => Rows = rows;
    public IReadOnlyList<IReadOnlyList<BotButton>> Rows { get; }
}
