using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ReminderAssistantBot.Telegram.Adapters;
using ReminderAssistantBot.Telegram.Options;
using ReminderAssistantBot.Telegram.Processing;
using ReminderAssistantBot.Telegram.SceneEngine;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace ReminderAssistantBot.Telegram.Hosting;

public static class ServiceExtensions
{
    public static IServiceCollection AddTelegramPolling(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<TelegramOptions>()
            .Bind(configuration.GetSection(TelegramOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Token), "Telegram token is required")
            .ValidateOnStart();

        services.AddHttpClient("tg_bot_client")
            .RemoveAllLoggers()
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                TelegramOptions telegram = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;
                TelegramBotClientOptions botOptions = new(telegram.Token);
                return new TelegramBotClient(botOptions, httpClient);
            });

        services.AddScoped<IBotClient, TelegramBotClientAdapter>();
        services.AddScoped<UpdateProcessor>();
        services.AddSingleton<IUpdateHandler, UpdateHandler>();
        services.AddHostedService<PollingHostedService>();

        return services;
    }
}
