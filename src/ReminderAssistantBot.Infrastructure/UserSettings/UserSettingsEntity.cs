namespace ReminderAssistantBot.Infrastructure.UserSettings;

internal sealed class UserSettingsEntity
{
    public long UserId { get; set; }
    public string TimezoneKey { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; }
}
