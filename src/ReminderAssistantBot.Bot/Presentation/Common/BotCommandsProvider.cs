using ReminderAssistantBot.Bot.Presentation.UI;
using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Bot.Presentation.Common;

internal sealed class BotCommandsProvider : IBotCommandsProvider
{
    public IReadOnlyList<BotCommandInfo> GetCommands() =>
    [
        new(UiStrings.Commands.Start, UiStrings.Buttons.BotStart),
        new(UiStrings.Commands.Menu, UiStrings.Buttons.BotMenu),
        new(UiStrings.Commands.About, UiStrings.Buttons.BotAbout)
    ];
}
