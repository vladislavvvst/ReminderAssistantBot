using Microsoft.Extensions.Logging;
using ReminderAssistantBot.Telegram.SceneEngine;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace ReminderAssistantBot.Telegram.Processing;

internal sealed class UpdateProcessor
{
    private readonly ILogger<UpdateProcessor> _logger;
    private readonly IStateCache _stateStorage;
    private readonly ISceneRegistry _sceneRegistry;
    private readonly ICommandRouter? _commandRouter;
    private readonly IBotClient _botClient;

    public UpdateProcessor(ILogger<UpdateProcessor> logger, IStateCache stateStorage, ISceneRegistry sceneRegistry,
        IEnumerable<ICommandRouter> commandRouters, IBotClient botClient)
    {
        _logger = logger;
        _stateStorage = stateStorage;
        _sceneRegistry = sceneRegistry;
        _commandRouter = commandRouters.FirstOrDefault();
        _botClient = botClient;
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken ct)
    {
        _logger.LogError(exception, "HandleError (source={Source})", source);

        if (exception is global::Telegram.Bot.Exceptions.RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), ct);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!IsAllowedAndPrivate(update))
            return;

        BotUpdate botUpdate = Map(update);
        UpdateContext context = new(_logger, _stateStorage, _botClient, botUpdate);

        if (_commandRouter != null && await _commandRouter.TryHandleAsync(context, ct))
            return;

        await SceneRouter.RouteAsync(context, _sceneRegistry, ct);
    }

    private static BotUpdate Map(Update update)
    {
        if (update.CallbackQuery is { } callback)
        {
            long userId = callback.Message?.Chat.Id ?? 0;
            return new BotUpdate(UpdateKind.Callback, userId, null, callback.Id, callback.Data);
        }

        if (update.Message is { } message)
        {
            long userId = message.Chat.Id;
            return new BotUpdate(UpdateKind.Message, userId, message.Text, null, null);
        }

        long fallbackUserId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id ?? 0;
        return new BotUpdate(UpdateKind.Other, fallbackUserId, null, null, null);
    }

    private static bool IsAllowedAndPrivate(Update update)
    {
        long? id = TryGetUserId(update);
        return id is not null && IsPrivate(update);
    }

    private static bool IsPrivate(Update update) =>
        (update.Message?.Chat.Type ?? update.CallbackQuery?.Message?.Chat.Type) == global::Telegram.Bot.Types.Enums.ChatType.Private;

    private static long? TryGetUserId(Update update) =>
        update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id;
}
