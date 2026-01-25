namespace ReminderAssistantBot.Bot.Options;

public sealed class ReminderOptions
{
    public const string SectionName = "Reminder";
    public TimeSpan TimeoutOperation { get; init; } = TimeSpan.FromSeconds(30); // default 30 sec
}
