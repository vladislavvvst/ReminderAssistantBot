using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReminderAssistantBot.Bot.Infrastructure.Caching;
using ReminderAssistantBot.Bot.Options;
using ReminderAssistantBot.Bot.Presentation.Common;
using ReminderAssistantBot.Bot.Presentation.Features;
using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Bot;

public static class ServiceExtensions
{
    public static IServiceCollection AddReminderBot(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ReminderOptions>()
            .Bind(configuration.GetSection(ReminderOptions.SectionName))
            .Validate(o => o.TimeoutOperation >= TimeSpan.FromSeconds(5), "Timeout operation must >= 5 seconds")
            .ValidateOnStart();

        services.AddMemoryCache();
        services.AddSingleton<IStateCache, StateMemoryCache>();
        services.AddSingleton<IUiStateCache, UiStateMemoryCache>();
        services.AddScoped<ISceneRegistry, SceneRegistry>();
        services.AddScoped<ICommandRouter, CommandRouter>();
        services.AddSingleton<IBotCommandsProvider, BotCommandsProvider>();

        services.AddScoped<MainMenuScene>();
        services.AddScoped<AddReminderScene>();
        services.AddScoped<ActiveRemindersScene>();
        services.AddScoped<DeleteReminderScene>();

        return services;
    }
}
