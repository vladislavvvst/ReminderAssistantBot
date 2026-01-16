namespace ReminderAssistantBot.Application.Abstractions;

public sealed class PersistenceUnavailableException(string message, Exception inner) : Exception(message, inner);
