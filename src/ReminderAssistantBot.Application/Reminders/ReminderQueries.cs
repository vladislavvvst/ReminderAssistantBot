using ReminderAssistantBot.Application.Abstractions;
using ReminderAssistantBot.Domain;

namespace ReminderAssistantBot.Application.Reminders;

internal sealed class ReminderQueries : IReminderQueries
{
    private readonly IReminderRepository _repository;

    public ReminderQueries(IReminderRepository repository) => _repository = repository;

    public Task<IReadOnlyList<Reminder>> GetActiveAsync(long userId, CancellationToken ct) =>
        _repository.GetActiveAsync(userId, ct);

}
