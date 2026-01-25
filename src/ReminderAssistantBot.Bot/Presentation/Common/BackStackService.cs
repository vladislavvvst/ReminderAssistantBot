using System.Collections.Concurrent;

namespace ReminderAssistantBot.Bot.Presentation.Common;

internal static class BackStackService
{
    private static readonly ConcurrentDictionary<long, Stack<string>?> Stacks = new();

    public static void Push(long userId, string currentKey)
    {
        Stack<string>? stack = Stacks.GetOrAdd(userId, static _ => new Stack<string>());
        stack?.Push(currentKey);
    }

    public static string? Pop(long userId)
    {
        if (Stacks.TryGetValue(userId, out Stack<string>? stack) && stack is { Count: > 0 })
            return stack.Pop();

        return null;
    }

    public static void Clear(long userId) => Stacks.TryRemove(userId, out Stack<string>? _);
}
