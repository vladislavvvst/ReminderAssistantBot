namespace ReminderAssistantBot.Telegram.SceneEngine;

public sealed record BotUpdate(UpdateKind Kind, long ChatId, string? Text, string? CallbackId, string? CallbackData);
