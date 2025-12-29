using ReminderAssistantBot.Application.Abstractions;

namespace ReminderAssistantBot.Application.Reminders;

internal sealed class DeleteReminder : IDeleteReminder
{
    private readonly IReminderRepository _repository;

    public DeleteReminder(IReminderRepository repository) => _repository = repository;

    public Task<bool> HandleAsync(long userId, Guid reminderId, CancellationToken ct) =>
        _repository.DeleteAsync(userId, reminderId, ct);
}
