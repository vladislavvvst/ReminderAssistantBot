namespace ReminderAssistantBot.Application.Abstractions;

public sealed record ClaimedReminder(Guid Id, long UserId, string Message, Guid LeaseToken);
