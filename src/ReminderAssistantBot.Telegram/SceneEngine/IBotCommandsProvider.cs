namespace ReminderAssistantBot.Telegram.SceneEngine;

public readonly record struct BotCommandInfo(string Command, string Description);

public interface IBotCommandsProvider
{
    IReadOnlyList<BotCommandInfo> GetCommands();
}
