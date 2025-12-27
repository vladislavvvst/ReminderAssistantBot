using ReminderAssistantBot.Bot;
using ReminderAssistantBot.Telegram.Hosting;

namespace ReminderAssistantBot.Worker;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddReminderBot();
        builder.Services.AddTelegramPolling(builder.Configuration);

        builder.ConfigureContainer(new DefaultServiceProviderFactory(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        }));

        IHost host = builder.Build();
        await host.RunAsync();
    }
}
