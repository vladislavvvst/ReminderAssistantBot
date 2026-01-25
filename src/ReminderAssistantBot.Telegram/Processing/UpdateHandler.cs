using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace ReminderAssistantBot.Telegram.Processing;

/// <summary>
/// Адаптер между Telegram-клиентом и приложением.
/// Реализует IUpdateHandler: на каждый update создает DI-scope и передает управление UpdateProcessor.
/// </summary>
internal sealed class UpdateHandler : IUpdateHandler
{
    private readonly IServiceScopeFactory _scopeFactory;

    public UpdateHandler(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        UpdateProcessor processor = scope.ServiceProvider.GetRequiredService<UpdateProcessor>();
        await processor.HandleUpdateAsync(botClient, update, ct);
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken ct)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        UpdateProcessor processor = scope.ServiceProvider.GetRequiredService<UpdateProcessor>();
        await processor.HandleErrorAsync(botClient, exception, source, ct);
    }
}
