using ReminderAssistantBot.Application.Abstractions;
using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Worker.Integrations.Telegram;

internal sealed class TelegramReminderSender(IBotClient bot) : IReminderSender
{
    public Task SendAsync(long userId, string message, CancellationToken ct)
    {
        // TODO: вынести в ui string по хорошему
        string text = $"🔔 <b>Напоминание</b>\n{message}";
        return bot.SendTextAsync(userId, text, ParseMode.Html, null, ct);
    }
}
