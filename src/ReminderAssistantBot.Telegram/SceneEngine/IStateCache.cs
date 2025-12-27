namespace ReminderAssistantBot.Telegram.SceneEngine;

public interface IStateCache
{
    Task<string> GetStateAsync(long chatId);
    Task SetStateAsync(long chatId, string stateKey);
}
