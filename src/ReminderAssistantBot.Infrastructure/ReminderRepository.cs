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

    public async Task<IReadOnlyList<Reminder>> GetDueAsync(DateTime utcNow, CancellationToken ct)
    {
        try
        {
            List<ReminderEntity> entities = await dbContext.Reminders
                .AsNoTracking()
                .Where(x => x.Status == ReminderStatus.Pending && x.DueAtUtc <= utcNow)
                .OrderBy(x => x.DueAtUtc)
                .ToListAsync(cancellationToken: ct);

            return entities.Select(Map).ToList();
        }
        catch (Exception ex) when (IsDbFailure(ex))
        {
            logger.LogError(ex, "Database get due reminders failed");
            throw new PersistenceUnavailableException("Database get due reminders failed", ex);
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

    public async Task<int> UpdateStatusAsync(Guid id, ReminderStatus status, DateTime? sentAtUtc, CancellationToken ct)
    {
        try
        {
            int affectedRows = await dbContext.Reminders
                .Where(x => x.Id == id && x.Status == ReminderStatus.Pending)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, status)
                    .SetProperty(x => x.SentAtUtc, sentAtUtc), ct);

            return affectedRows;
        }
        catch (Exception ex) when (IsDbFailure(ex))
        {
            logger.LogError(ex, "Database update reminder failed. ReminderId={ReminderId}, Status={Status}, SentAtUtc={SentAtUtc}", id, status, sentAtUtc);
            throw new PersistenceUnavailableException("Database update reminder failed", ex);
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

    public async Task<string> GetTimezoneAsync(long userId, TimeSpan timeout, CancellationToken ct)
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
            Id           = reminder.Id,
            UserId       = reminder.UserId,
            Message      = reminder.Message,
            DueAtUtc     = reminder.DueAtUtc,
            SentAtUtc    = reminder.SentAtUtc,
            Status       = reminder.Status,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private static Reminder Map(ReminderEntity reminderEntity)
    {
        return Reminder.Rehydrate
        (
            id:         reminderEntity.Id,
            userId:     reminderEntity.UserId,
            message:    reminderEntity.Message,
            dueAtUtc:   reminderEntity.DueAtUtc,
            sentAtUtc:  reminderEntity.SentAtUtc,
            status:     reminderEntity.Status
        );
    }

    private static UserSettingsEntity Map(long userId, string timezoneKey)
    {
        return new UserSettingsEntity
        {
            UserId          = userId,
            TimezoneKey     = timezoneKey,
            UpdatedAtUtc    = DateTime.UtcNow
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
}
