using NodaTime;
using NodaTime.Text;
using ReminderAssistantBot.Application.Reminders;
using ReminderAssistantBot.Bot.Presentation.UI;
using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Bot.Presentation.Common;

internal static class SceneUiUtils
{
    private const string DefaultTimezone = "Europe/Moscow";
    private static readonly LocalDateTimePattern DateTimePattern =
        LocalDateTimePattern.CreateWithInvariantCulture("dd.MM.yyyy HH:mm");

    public static DateTimeZone ResolveTimezone(string? tzId)
    {
        if (string.IsNullOrWhiteSpace(tzId) || !DateTimeZoneProviders.Tzdb.Ids.Contains(tzId))
            tzId = DefaultTimezone;

        return DateTimeZoneProviders.Tzdb[tzId];
    }

    public static string FormatUtc(DateTime utcDateTime, DateTimeZone zone)
    {
        Instant instant = Instant.FromDateTimeUtc(utcDateTime);
        LocalDateTime local = instant.InZone(zone).LocalDateTime;
        return DateTimePattern.Format(local);
    }

    public static DateTime ToUtc(DateTime localDateTime, DateTimeZone zone)
    {
        LocalDateTime ldt = LocalDateTime.FromDateTime(localDateTime);
        return ldt.InZoneLeniently(zone).ToInstant().ToDateTimeUtc();
    }

    public static bool TryHandleStatus(OperationStatus status, out string errorMessage)
    {
        if (status is OperationStatus.Success)
        {
            errorMessage = string.Empty;
            return true;
        }

        errorMessage = status switch
        {
            OperationStatus.Timeout                 => CommonUiStrings.Errors.Timeout,
            OperationStatus.Unavailable             => CommonUiStrings.Errors.Unavailable,
            OperationStatus.NotFound                => CommonUiStrings.Errors.NotFound,
            OperationStatus.Rejected                => CommonUiStrings.Errors.Rejected,
            OperationStatus.ValidationInputFormat   => CommonUiStrings.Errors.ValidationInputFormat,
            OperationStatus.ValidationInputDate     => CommonUiStrings.Errors.ValidationInputDate,
            _                                       => CommonUiStrings.Errors.UnknownError
        };

        return false;
    }

    public static Task SendSuccessAsync(UpdateContext context, CancellationToken ct)
    {
        return context.Bot.SendTextAsync(context.Update.UserId, CommonUiStrings.Prompts.Success, ParseMode.Html, null, ct);
    }

    public static Task SendErrorAsync(UpdateContext context, string errorMessage, CancellationToken ct)
    {
        return context.Bot.SendTextAsync(context.Update.UserId, errorMessage, ParseMode.None, null, ct);
    }
}
