using Microsoft.Extensions.Caching.Memory;
using ReminderAssistantBot.Bot.Presentation.Common;
using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Bot.Infrastructure.Caching;

internal sealed class StateMemoryCache : IStateCache
{
    private readonly IMemoryCache _cache;

    public StateMemoryCache(IMemoryCache cache) => _cache = cache;

    public Task<string> GetStateAsync(long chatId)
    {
        string cacheKey = BuildStateKey(chatId);
        return Task.FromResult(_cache.TryGetValue(cacheKey, out string? stateKey)
            ? stateKey!
            : SceneKeys.MainMenu);
    }

    public Task SetStateAsync(long chatId, string stateKey)
    {
        string cacheKey = BuildStateKey(chatId);
        _cache.Set(cacheKey, stateKey);
        return Task.CompletedTask;
    }

    private static string BuildStateKey(long chatId) => $"state:{chatId}";
}
