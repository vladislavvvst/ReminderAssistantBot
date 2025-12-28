using ReminderAssistantBot.Domain;

namespace ReminderAssistantBot.Infrastructure;

internal sealed class ReminderEntity
{
    public Guid Id { get; set; }
    public long UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime DueAtUtc { get; set; }
    public ReminderStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
}
