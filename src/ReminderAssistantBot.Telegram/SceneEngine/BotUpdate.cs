namespace ReminderAssistantBot.Telegram.SceneEngine;

public sealed record BotUpdate(UpdateKind Kind, long UserId, string? Text, string? CallbackId, string? CallbackData);
