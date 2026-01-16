namespace ReminderAssistantBot.Application.Reminders;

public enum OperationStatus
{
    Success,
    NotFound,
    Timeout,
    Unavailable,
    Rejected,
    ValidationInputFormat,
    ValidationInputDate,
}
