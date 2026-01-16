using Microsoft.Extensions.Logging;
using ReminderAssistantBot.Application.Abstractions;
using ReminderAssistantBot.Domain;

namespace ReminderAssistantBot.Application.Reminders;

internal sealed class ReminderService(IReminderRepository repository, ILogger<ReminderService> logger) : IReminderService
{
    public async Task<OperationStatus> CreateAsync(long userId, string message, DateTime dueAtUtc, TimeSpan timeout, CancellationToken ct)
    {
        Reminder reminder;

        try
        {
            reminder = new Reminder(userId, message, dueAtUtc);
        }
        catch (ArgumentException aex)
        {
            if (aex.GetBaseException() is ArgumentOutOfRangeException)
            {
                logger.LogWarning(aex, "Reminder due time must be in the future. UserId={UserId}", userId);
                return OperationStatus.ValidationInputDate;
            }

            logger.LogWarning("Reminder is invalid. UserId={UserId}", userId);
            return OperationStatus.ValidationInputFormat;
        }

        using CancellationTokenSource ctsTimeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        ctsTimeout.CancelAfter(timeout);

        try
        {
            await repository.AddAsync(reminder, ctsTimeout.Token);
            return OperationStatus.Success;
        }
        catch (OperationCanceledException) when (ctsTimeout.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            logger.LogError("Reminder add timeout. UserId={UserId}", userId);
            return OperationStatus.Timeout;
        }
        catch (PersistenceUnavailableException)
        {
            return OperationStatus.Unavailable;
        }
    }

    public async Task<OperationStatus> DeleteAsync(long userId, Guid reminderId, TimeSpan timeout, CancellationToken ct)
    {
        using CancellationTokenSource ctsTimeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        ctsTimeout.CancelAfter(timeout);

        try
        {
            int affectedRows = await repository.DeleteAsync(userId, reminderId, ctsTimeout.Token);
            return affectedRows == 0 ? OperationStatus.NotFound : OperationStatus.Success;
        }
        catch (OperationCanceledException) when (ctsTimeout.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            logger.LogError("Reminder delete timeout. UserId={UserId}", userId);
            return OperationStatus.Timeout;
        }
        catch (PersistenceUnavailableException)
        {
            return OperationStatus.Unavailable;
        }
    }

    public async Task<(OperationStatus, IReadOnlyList<Reminder>)> GetActiveAsync(long userId, TimeSpan timeout, CancellationToken ct)
    {
        using CancellationTokenSource ctsTimeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        ctsTimeout.CancelAfter(timeout);

        try
        {
            IReadOnlyList<Reminder> reminders = await repository.GetActiveAsync(userId, ctsTimeout.Token);
            return (OperationStatus.Success, reminders);
        }
        catch (OperationCanceledException) when (ctsTimeout.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            logger.LogError("Reminder get active timeout. UserId={UserId}", userId);
            return (OperationStatus.Timeout, []);
        }
        catch (PersistenceUnavailableException)
        {
            return (OperationStatus.Unavailable, []);
        }
    }
}
