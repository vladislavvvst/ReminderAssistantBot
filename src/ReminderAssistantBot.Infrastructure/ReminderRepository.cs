using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReminderAssistantBot.Application.Abstractions;
using ReminderAssistantBot.Domain;
using ReminderAssistantBot.Infrastructure.Reminders;
using ReminderAssistantBot.Infrastructure.UserSettings;
using System.Data.Common;

namespace ReminderAssistantBot.Infrastructure;

internal sealed class ReminderRepository(ILogger<ReminderRepository> logger, ApplicationDbContext dbContext) : IReminderRepository
{
    private const int MaxClaimBatchSize = 500;

    public async Task AddAsync(Reminder reminder, CancellationToken ct)
    {
        try
        {
            await dbContext.Reminders.AddAsync(Map(reminder), ct);
            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (IsDbFailure(ex))
        {
            logger.LogError(ex, "Database write reminder failed. UserId={UserId}, ReminderId={ReminderId}", reminder.UserId, reminder.Id);
            throw new PersistenceUnavailableException("Database write reminder failed", ex);
        }
    }

    public async Task<IReadOnlyList<ClaimedReminder>> ClaimDueAsync(DateTime utcNow, TimeSpan leaseDuration, int batchSize, CancellationToken ct)
    {
        if (leaseDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(leaseDuration), "Lease duration must be positive");

        if (batchSize <= 0)
            return [];

        int takeBatchSize = Math.Min(batchSize, MaxClaimBatchSize);
        DateTime leaseUntilUtc = utcNow.Add(leaseDuration);

        try
        {
            List<CandidateReminder> candidates = await dbContext.Reminders
                .AsNoTracking()
                .Where(x => x.DueAtUtc <= utcNow &&
                            (x.Status == ReminderStatus.Pending ||
                             (x.Status == ReminderStatus.Processing && x.LeaseUntilUtc < utcNow)))
                .OrderBy(x => x.DueAtUtc)
                .Take(takeBatchSize)
                .Select(x => new CandidateReminder(x.Id, x.UserId, x.Message))
                .ToListAsync(ct);

            List<ClaimedReminder> claimed = new(candidates.Count);

            foreach (CandidateReminder candidate in candidates)
            {
                Guid leaseToken = Guid.NewGuid();

                int affectedRows = await dbContext.Reminders
                    .Where(x => x.Id == candidate.Id
                        && x.DueAtUtc <= utcNow
                        && (x.Status == ReminderStatus.Pending
                            || (x.Status == ReminderStatus.Processing && x.LeaseUntilUtc < utcNow)))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.Status, ReminderStatus.Processing)
                        .SetProperty(x => x.LeaseToken, leaseToken)
                        .SetProperty(x => x.LeaseUntilUtc, leaseUntilUtc)
                        .SetProperty(x => x.AttemptCount, x => x.AttemptCount + 1), ct);

                if (affectedRows == 1)
                    claimed.Add(new ClaimedReminder(candidate.Id, candidate.UserId, candidate.Message, leaseToken));
            }

            return claimed;
        }
        catch (Exception ex) when (IsDbFailure(ex))
        {
            logger.LogError(ex, "Database claim due reminders failed");
            throw new PersistenceUnavailableException("Database claim due reminders failed", ex);
        }
    }

    public async Task<IReadOnlyList<Reminder>> GetActiveAsync(long userId, CancellationToken ct)
    {
        try
        {
            List<ReminderEntity> entities = await dbContext.Reminders
                .AsNoTracking()
                .Where(x => x.Status == ReminderStatus.Pending && x.UserId == userId)
                .OrderBy(x => x.DueAtUtc)
                .ToListAsync(ct);

            return entities.Select(Map).ToList();
        }
        catch (Exception ex) when (IsDbFailure(ex))
        {
            logger.LogError(ex, "Database get active reminders failed");
            throw new PersistenceUnavailableException("Database get active reminders failed", ex);
        }
    }

    public async Task<int> DeleteAsync(long userId, Guid reminderId, CancellationToken ct)
    {
        try
        {
            int affectedRows = await dbContext.Reminders
                .Where(x => x.Id == reminderId && x.UserId == userId && x.Status == ReminderStatus.Pending)
                .ExecuteDeleteAsync(cancellationToken: ct);

            return affectedRows;
        }
        catch (Exception ex) when (IsDbFailure(ex))
        {
            logger.LogError(ex, "Database delete reminder failed. UserId={UserId}, ReminderId={ReminderId}", userId, reminderId);
            throw new PersistenceUnavailableException("Database delete reminder failed", ex);
        }
    }

    public async Task<int> MarkSentAsync(Guid id, Guid leaseToken, DateTime sentAtUtc, CancellationToken ct)
    {
        try
        {
            return await dbContext.Reminders
                .Where(x => x.Id == id
                    && x.Status == ReminderStatus.Processing
                    && x.LeaseToken == leaseToken)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, ReminderStatus.Sent)
                    .SetProperty(x => x.SentAtUtc, sentAtUtc)
                    .SetProperty(x => x.LeaseToken, (Guid?)null)
                    .SetProperty(x => x.LeaseUntilUtc, (DateTime?)null), ct);
        }
        catch (Exception ex) when (IsDbFailure(ex))
        {
            logger.LogError(ex, "Database mark sent reminder failed. ReminderId={ReminderId}, LeaseToken={LeaseToken}", id, leaseToken);
            throw new PersistenceUnavailableException("Database mark sent reminder failed", ex);
        }
    }

    public async Task AddOrUpdateTimezoneAsync(long userId, string timezoneKey, CancellationToken ct)
    {
        try
        {
            UserSettingsEntity? entity = await dbContext.UserSettings
                .FirstOrDefaultAsync(x => x.UserId == userId, ct);

            if (entity is null)
                await dbContext.UserSettings.AddAsync(Map(userId, timezoneKey), ct);
            else
                entity.TimezoneKey = timezoneKey;

            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (IsDbFailure(ex))
        {
            logger.LogError(ex, "Database add or update timezone key failed. UserId={UserId}", userId);
            throw new PersistenceUnavailableException("Database write timezone key failed", ex);
        }
    }

    public async Task<string> GetTimezoneAsync(long userId, CancellationToken ct)
    {
        try
        {
            return await dbContext.UserSettings
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => x.TimezoneKey)
                .FirstOrDefaultAsync(ct) ?? string.Empty;
        }
        catch (Exception ex) when (IsDbFailure(ex))
        {
            logger.LogError(ex, "Database get timezone key failed. UserId={UserId}", userId);
            throw new PersistenceUnavailableException("Database get timezone key failed", ex);
        }
    }

    private static ReminderEntity Map(Reminder reminder)
    {
        return new ReminderEntity
        {
            Id = reminder.Id,
            UserId = reminder.UserId,
            Message = reminder.Message,
            DueAtUtc = reminder.DueAtUtc,
            SentAtUtc = reminder.SentAtUtc,
            Status = reminder.Status,
            CreatedAtUtc = DateTime.UtcNow,
            LeaseToken = null,
            LeaseUntilUtc = null,
            AttemptCount = 0
        };
    }

    private static Reminder Map(ReminderEntity reminderEntity)
    {
        return Reminder.Rehydrate
        (
            id: reminderEntity.Id,
            userId: reminderEntity.UserId,
            message: reminderEntity.Message,
            dueAtUtc: reminderEntity.DueAtUtc,
            sentAtUtc: reminderEntity.SentAtUtc,
            status: reminderEntity.Status
        );
    }

    private static UserSettingsEntity Map(long userId, string timezoneKey)
    {
        return new UserSettingsEntity
        {
            UserId = userId,
            TimezoneKey = timezoneKey,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    private static bool IsDbFailure(Exception ex)
    {
        return ex switch
        {
            OperationCanceledException => false,
            DbUpdateException or DbUpdateConcurrencyException or DbException or TimeoutException => true,
            _ => ex.InnerException is not null && IsDbFailure(ex.InnerException)
        };
    }

    private sealed record CandidateReminder(Guid Id, long UserId, string Message);
}
