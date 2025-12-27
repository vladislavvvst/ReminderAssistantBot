using Microsoft.Extensions.DependencyInjection;
using ReminderAssistantBot.Bot.Infrastructure.Caching;
using ReminderAssistantBot.Bot.Presentation.Common;
using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Bot;

public static class ServiceExtensions
{
    public static IServiceCollection AddReminderBot(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<IStateCache, StateMemoryCache>();
        services.AddSingleton<ISceneRegistry, SceneRegistryAdapter>();
        services.AddSingleton<ICommandRouter, CommandRouter>();
        services.AddSingleton<IBotCommandsProvider, BotCommandsProvider>();

        SceneRegistry.Bootstrap();

        return services;
    }
}
