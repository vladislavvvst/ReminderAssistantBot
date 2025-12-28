using ReminderAssistantBot.Application;
using ReminderAssistantBot.Application.Reminders;
using ReminderAssistantBot.Bot;
using ReminderAssistantBot.Infrastructure;
using ReminderAssistantBot.Telegram.Hosting;
using ReminderAssistantBot.Worker.HostedServices;
using ReminderAssistantBot.Worker.Integrations.Telegram;

namespace ReminderAssistantBot.Worker;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddReminderBot();
        builder.Services.AddTelegramPolling(builder.Configuration);

        builder.Services.AddScoped<IReminderSender, TelegramReminderSender>();
        builder.Services.AddHostedService<ReminderDispatchWorker>();

        builder.ConfigureContainer(new DefaultServiceProviderFactory(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        }));

        IHost host = builder.Build();
        await host.RunAsync();
    }
}
