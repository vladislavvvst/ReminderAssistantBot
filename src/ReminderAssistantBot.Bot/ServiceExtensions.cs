using Microsoft.Extensions.DependencyInjection;
using ReminderAssistantBot.Bot.Infrastructure.Caching;
using ReminderAssistantBot.Bot.Presentation.Common;
using ReminderAssistantBot.Bot.Presentation.Features;
using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Bot;

public static class ServiceExtensions
{
    public static IServiceCollection AddReminderBot(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<IStateCache, StateMemoryCache>();
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
