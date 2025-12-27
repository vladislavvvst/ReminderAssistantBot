using System.Collections.Concurrent;

namespace ReminderAssistantBot.Bot.Presentation.Common;

internal static class BackStackService
{
    private static readonly ConcurrentDictionary<long, Stack<string>?> Stacks = new();

    public static void Push(long chatId, string currentKey)
    {
        Stack<string>? stack = Stacks.GetOrAdd(chatId, static _ => new Stack<string>());
        stack?.Push(currentKey);
    }

    public static string? Pop(long chatId)
    {
        if (Stacks.TryGetValue(chatId, out Stack<string>? stack) && stack is { Count: > 0 })
            return stack.Pop();

        return null;
    }

    public static void Clear(long chatId) => Stacks.TryRemove(chatId, out Stack<string>? _);
}
