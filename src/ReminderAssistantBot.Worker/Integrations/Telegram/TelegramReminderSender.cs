using ReminderAssistantBot.Application.Reminders;
using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Worker.Integrations.Telegram;

internal sealed class TelegramReminderSender : IReminderSender
{
    private readonly IBotClient _bot;

    public TelegramReminderSender(IBotClient bot) => _bot = bot;

    public Task SendAsync(long userId, string message, CancellationToken ct)
    {
        // TODO: вынести в ui string по хорошему
        string text = $"🔔 <b>Напоминание</b>\n{message}";
        return _bot.SendTextAsync(userId, text, ParseMode.Html, null, ct);
    }
}
