using Microsoft.Extensions.Logging;

namespace ReminderAssistantBot.Telegram.SceneEngine;

public sealed record UpdateContext(ILogger Logger, IStateCache StateCache, IBotClient Bot, BotUpdate Update);
