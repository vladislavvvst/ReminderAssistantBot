using ReminderAssistantBot.Application.Reminders;

namespace ReminderAssistantBot.Worker.HostedServices;

internal sealed class ReminderDispatchWorker(ILogger<ReminderDispatchWorker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        PeriodicTimer timer = new(TimeSpan.FromSeconds(10));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            stoppingToken.ThrowIfCancellationRequested();

            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                DispatchDueReminders handler = scope.ServiceProvider.GetRequiredService<DispatchDueReminders>();

                OperationStatus status = await handler.HandleAsync(TimeSpan.FromSeconds(5), stoppingToken);
                if (status != OperationStatus.Success)
                    logger.LogError("Dispatch reminder failed: {Status}", status);

            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                /* ignore */
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception");
            }
        }
    }
}
