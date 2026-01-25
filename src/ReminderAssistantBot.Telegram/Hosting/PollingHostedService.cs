using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReminderAssistantBot.Telegram.SceneEngine;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace ReminderAssistantBot.Telegram.Hosting;

/// <summary>
/// Hosted-сервис, который запускает polling Telegram-бота в фоновом потоке.
/// Регистрируется в DI как IHostedService, стартует при запуске приложения.
/// </summary>
internal sealed class PollingHostedService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<PollingHostedService> _logger;
    private readonly IUpdateHandler _updateHandler;
    private readonly IBotCommandsProvider? _commandsProvider;

    public PollingHostedService(ITelegramBotClient botClient, IUpdateHandler updateHandler,
        ILogger<PollingHostedService> logger, IEnumerable<IBotCommandsProvider> commandsProviders)
    {
        _logger = logger;
        _updateHandler = updateHandler;
        _botClient = botClient;
        _commandsProvider = commandsProviders.FirstOrDefault();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting polling service");

        await TryEnsureCommandsAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                ReceiverOptions receiverOptions = new() { DropPendingUpdates = true, AllowedUpdates = [] };

                await _botClient.ReceiveAsync
                (
                    updateHandler: _updateHandler,
                    receiverOptions: receiverOptions,
                    cancellationToken: stoppingToken
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Polling failed with exception");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task TryEnsureCommandsAsync(CancellationToken ct)
    {
        if (_commandsProvider is null)
            return;

        IReadOnlyList<BotCommandInfo> commands = _commandsProvider.GetCommands();

        if (commands.Count == 0)
            return;

        try
        {
            await _botClient.SetMyCommands(commands.Select(c => new BotCommand { Command = c.Command, Description = c.Description }),
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set bot commands");
        }
    }
}
