using ReminderAssistantBot.Bot.Presentation.UI;
using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Bot.Presentation.Common;

internal sealed class BotCommandsProvider : IBotCommandsProvider
{
    public IReadOnlyList<BotCommandInfo> GetCommands() =>
    [
        new(CommonUiStrings.Commands.Start, CommonUiStrings.Buttons.BotStart),
        new(CommonUiStrings.Commands.Menu,  CommonUiStrings.Buttons.BotMenu),
        new(CommonUiStrings.Commands.About, CommonUiStrings.Buttons.BotAbout)
    ];
}
