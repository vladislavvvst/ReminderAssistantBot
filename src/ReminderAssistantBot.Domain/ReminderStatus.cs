namespace ReminderAssistantBot.Domain;

public enum ReminderStatus
{
    Pending,    // Ожидает отправки
    Processing, // В процессе отправки
    Sent        // Отправлено
}
