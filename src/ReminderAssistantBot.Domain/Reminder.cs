namespace ReminderAssistantBot.Domain;

/// <summary>
/// Событие напоминания (что напомнить, когда и кому).
/// </summary>
public sealed class Reminder
{
    public Guid Id { get; private set; }
    public long UserId { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public DateTime DueAtUtc { get; private set; }
    public DateTime? SentAtUtc { get; private set; }
    public ReminderStatus Status { get; private set; } = ReminderStatus.Pending;

    private Reminder() { }

    public Reminder(long userId, string message, DateTime dueAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (dueAtUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("DueAtUtc must be in UTC", nameof(dueAtUtc));

        if (dueAtUtc <= DateTime.UtcNow)
            throw new ArgumentOutOfRangeException(nameof(dueAtUtc), "Due time must be in the future");

        Id = Guid.NewGuid();
        UserId = userId;
        Message = message;
        DueAtUtc = dueAtUtc;
        Status = ReminderStatus.Pending;
    }

    public static Reminder Rehydrate(Guid id, long userId, string message, DateTime dueAtUtc, DateTime? sentAtUtc, ReminderStatus status)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId));

        if (id == Guid.Empty)
            throw new ArgumentException("Id is required", nameof(id));

        if (dueAtUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("DueAtUtc must be in UTC", nameof(dueAtUtc));

        if (sentAtUtc.HasValue && sentAtUtc.Value.Kind != DateTimeKind.Utc)
            throw new ArgumentException("SentAtUtc must be in UTC", nameof(sentAtUtc));

        if (status == ReminderStatus.Sent && sentAtUtc is null)
            throw new ArgumentException("SentAtUtc is required when status is Sent", nameof(sentAtUtc));

        if (status != ReminderStatus.Sent && sentAtUtc is not null)
            throw new ArgumentException("SentAtUtc must be null when status is not Sent", nameof(sentAtUtc));

        return new Reminder
        {
            Id = id,
            UserId = userId,
            Message = message,
            DueAtUtc = dueAtUtc,
            SentAtUtc = sentAtUtc,
            Status = status
        };
    }

    public void MarkSent()
    {
        if (Status == ReminderStatus.Sent)
            return;

        Status = ReminderStatus.Sent;
        SentAtUtc = DateTime.UtcNow;
    }
}
