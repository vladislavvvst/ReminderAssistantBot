using Microsoft.Extensions.Logging;
using ReminderAssistantBot.Bot.Presentation.Common;
using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Bot.Presentation.UI;

internal static class UiKeyboard
{
    public static async Task ClearPreviousAsync(UpdateContext context, IUiStateCache cache, CancellationToken ct)
    {
        long userId = context.Update.UserId;

        int? messageId = await cache.GetLastKeyboardMessageIdAsync(userId);
        if (messageId is null)
            return;

        await cache.ClearLastKeyboardMessageIdAsync(userId);

        try
        {
            await context.Bot.ClearKeyboardAsync(userId, messageId.Value, ct);
        }
        catch (Exception ex)
        {
            context.Logger.LogDebug(ex, "Failed to clear inline keyboard");
        }
    }

    public static async Task<int> SendAndTrackAsync(UpdateContext context, IUiStateCache cache, string text,
        ParseMode parseMode, BotInlineKeyboard keyboard, CancellationToken ct)
    {
        int messageId = await context.Bot.SendTextWithIdAsync(context.Update.UserId, text, parseMode, keyboard, ct);
        await cache.SetLastKeyboardMessageIdAsync(context.Update.UserId, messageId);
        return messageId;
    }
}
