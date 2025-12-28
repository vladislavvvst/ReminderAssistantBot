using ReminderAssistantBot.Application.Reminders;

namespace ReminderAssistantBot.Worker.HostedServices;

internal sealed class ReminderDispatchWorker : BackgroundService
{
    private readonly ILogger<ReminderDispatchWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ReminderDispatchWorker(ILogger<ReminderDispatchWorker> logger, IServiceScopeFactory scopeFactory)
        => (_logger, _scopeFactory) = (logger, scopeFactory);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        PeriodicTimer timer = new(TimeSpan.FromSeconds(10));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            stoppingToken.ThrowIfCancellationRequested();

            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                DispatchDueReminders handler = scope.ServiceProvider.GetRequiredService<DispatchDueReminders>();
                await handler.HandleAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                /* ignore */
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
            }
        }
    }
}
