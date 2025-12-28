using ReminderAssistantBot.Application.Abstractions;
using ReminderAssistantBot.Domain;

namespace ReminderAssistantBot.Application.Reminders;

public sealed class CreateReminder : ICreateReminder
{
    private readonly IReminderRepository _repository;

    public CreateReminder(IReminderRepository repository) => _repository = repository;

    public async Task HandleAsync(long userId, string message, DateTime dueAtUtc, CancellationToken ct)
    {
        Reminder reminder = new(userId, message, dueAtUtc);

        await _repository.AddAsync(reminder, ct);
        await _repository.SaveChangesAsync(ct);
    }
}
