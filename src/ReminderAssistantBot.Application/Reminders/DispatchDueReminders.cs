using Microsoft.Extensions.Logging;
using ReminderAssistantBot.Application.Abstractions;

namespace ReminderAssistantBot.Application.Reminders;

public sealed class DispatchDueReminders(IReminderRepository repository, IReminderSender sender, ILogger<DispatchDueReminders> logger)
{
    private const int ClaimBatchSize = 100;
    private static readonly TimeSpan MinimumLeaseDuration = TimeSpan.FromSeconds(30);

    public async Task<OperationStatus> HandleAsync(TimeSpan timeout, CancellationToken ct)
    {
        TimeSpan leaseDuration = CalculateLeaseDuration(timeout);

        using CancellationTokenSource ctsClaimTimeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        ctsClaimTimeout.CancelAfter(timeout);

        IReadOnlyList<ClaimedReminder> due;

        try
        {
            due = await repository.ClaimDueAsync(DateTime.UtcNow, leaseDuration, ClaimBatchSize, ctsClaimTimeout.Token);
        }
        catch (OperationCanceledException) when (ctsClaimTimeout.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            return OperationStatus.Timeout;
        }
        catch (PersistenceUnavailableException)
        {
            return OperationStatus.Unavailable;
        }

        foreach (ClaimedReminder reminder in due)
        {
            using CancellationTokenSource ctsSendTimeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            ctsSendTimeout.CancelAfter(timeout);

            try
            {
                await sender.SendAsync(reminder.UserId, reminder.Message, ctsSendTimeout.Token);
            }
            catch (OperationCanceledException) when (ctsSendTimeout.IsCancellationRequested && !ct.IsCancellationRequested)
            {
                logger.LogWarning("Send reminder timed out. ReminderId={ReminderId}, LeaseToken={LeaseToken}", reminder.Id, reminder.LeaseToken);
                continue;
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                logger.LogError(ex, "Send reminder failed. ReminderId={ReminderId}, LeaseToken={LeaseToken}", reminder.Id, reminder.LeaseToken);
                continue;
            }

            using CancellationTokenSource ctsMarkTimeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            ctsMarkTimeout.CancelAfter(timeout);

            try
            {
                int affectedRows = await repository.MarkSentAsync(reminder.Id, reminder.LeaseToken, DateTime.UtcNow, ctsMarkTimeout.Token);

                if (affectedRows == 0)
                    logger.LogWarning("MarkSent skipped because lease is no longer valid. ReminderId={ReminderId}, LeaseToken={LeaseToken}",
                        reminder.Id, reminder.LeaseToken);
            }
            catch (OperationCanceledException) when (ctsMarkTimeout.IsCancellationRequested && !ct.IsCancellationRequested)
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

    private static TimeSpan CalculateLeaseDuration(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero)
            return MinimumLeaseDuration;

        TimeSpan doubledTimeout = timeout + timeout;
        return doubledTimeout < MinimumLeaseDuration ? MinimumLeaseDuration : doubledTimeout;
    }
}
