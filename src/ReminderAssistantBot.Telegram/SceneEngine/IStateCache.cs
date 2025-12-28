namespace ReminderAssistantBot.Telegram.SceneEngine;

public interface IStateCache
{
    Task<string> GetStateAsync(long userId);
    Task SetStateAsync(long userId, string stateKey);
}
