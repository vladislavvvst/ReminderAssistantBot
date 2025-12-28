using ReminderAssistantBot.Application.Abstractions;
using ReminderAssistantBot.Domain;

namespace ReminderAssistantBot.Application.Reminders;

public sealed class DispatchDueReminders
{
    private readonly IReminderRepository _repository;
    private readonly IReminderSender _sender;

    public DispatchDueReminders(IReminderRepository repository, IReminderSender sender)
        => (_repository, _sender) = (repository, sender);

    public async Task HandleAsync(CancellationToken ct)
    {
        IReadOnlyList<Reminder> due = await _repository.GetDueAsync(DateTime.UtcNow, ct);

        foreach (Reminder reminder in due)
        {
            await _sender.SendAsync(reminder.UserId, reminder.Message, ct);
            reminder.MarkSent();
            await _repository.UpdateStatusAsync(reminder.Id, reminder.Status, DateTime.UtcNow, ct);
        }
    }
}
