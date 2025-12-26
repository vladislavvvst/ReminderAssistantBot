namespace ReminderAssistantBot.Worker;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();

        IHost host = builder.Build();
        await host.RunAsync();
    }
}
