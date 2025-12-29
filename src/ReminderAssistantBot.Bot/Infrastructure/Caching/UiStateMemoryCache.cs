using Microsoft.Extensions.Caching.Memory;
using ReminderAssistantBot.Bot.Presentation.Common;

namespace ReminderAssistantBot.Bot.Infrastructure.Caching;

internal sealed class UiStateMemoryCache : IUiStateCache
{
    private readonly IMemoryCache _cache;

    public UiStateMemoryCache(IMemoryCache cache) => _cache = cache;

    public Task<int?> GetLastKeyboardMessageIdAsync(long userId)
    {
        string key = BuildKey(userId);
        return Task.FromResult(_cache.TryGetValue(key, out int messageId) ? (int?)messageId : null);
    }

    public Task SetLastKeyboardMessageIdAsync(long userId, int messageId)
    {
        _cache.Set(BuildKey(userId), messageId);
        return Task.CompletedTask;
    }

    public Task ClearLastKeyboardMessageIdAsync(long userId)
    {
        _cache.Remove(BuildKey(userId));
        return Task.CompletedTask;
    }

    private static string BuildKey(long userId) => $"ui:last-kb:{userId}";
}
