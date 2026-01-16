using ReminderAssistantBot.Application.Abstractions;
using ReminderAssistantBot.Domain;

namespace ReminderAssistantBot.Application.Reminders;

public sealed class DispatchDueReminders(IReminderRepository repository, IReminderSender sender)
{
    public async Task<OperationStatus> HandleAsync(TimeSpan timeout, CancellationToken ct)
    {
        using CancellationTokenSource ctsGetTimeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        ctsGetTimeout.CancelAfter(timeout);

        IReadOnlyList<Reminder> due;

        try
        {
            due = await repository.GetDueAsync(DateTime.UtcNow, ctsGetTimeout.Token);
        }
        catch (OperationCanceledException) when (ctsGetTimeout.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            return OperationStatus.Timeout;
        }
        catch (PersistenceUnavailableException)
        {
            return OperationStatus.Unavailable;
        }

        foreach (Reminder reminder in due)
        {
            using CancellationTokenSource ctsUpdateTimeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            ctsUpdateTimeout.CancelAfter(timeout);

            await sender.SendAsync(reminder.UserId, reminder.Message, ctsUpdateTimeout.Token);
            reminder.MarkSent();

            try
            {
                await repository.UpdateStatusAsync(reminder.Id, reminder.Status, DateTime.UtcNow, ctsUpdateTimeout.Token);
            }
            catch (OperationCanceledException) when (ctsUpdateTimeout.IsCancellationRequested && !ct.IsCancellationRequested)
            {
                return OperationStatus.Timeout;
            }
            catch (PersistenceUnavailableException)
            {
                return OperationStatus.Unavailable;
            }
        }

        return OperationStatus.Success;
    }
}
