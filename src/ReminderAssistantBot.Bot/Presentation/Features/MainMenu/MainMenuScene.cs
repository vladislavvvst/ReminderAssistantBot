using ReminderAssistantBot.Bot.Presentation.Common;
using ReminderAssistantBot.Bot.Presentation.UI;
using ReminderAssistantBot.Telegram.SceneEngine;

namespace ReminderAssistantBot.Bot.Presentation.Features.MainMenu;

internal sealed class MainMenuScene : IScene
{
    public string StateKey => SceneKeys.MainMenu;

    public async Task EnterAsync(UpdateContext context, CancellationToken ct)
    {
        BackStackService.Clear(context.Update.ChatId);
        await context.Bot.SendTextAsync(context.Update.ChatId, "ПШЕЛ ВОН ОТ СЮДА НЕ ГОТОВО НИХУЯ ЕЩЕ", ParseMode.None, null, ct);
    }

    public async Task OnMessageAsync(UpdateContext context, CancellationToken ct)
    {
        await context.Bot.SendTextAsync(context.Update.ChatId, UiStrings.Errors.UnknownCmd, ParseMode.None, null, ct);
    }

    public async Task OnCallbackAsync(UpdateContext context, CancellationToken ct)
    {
        string data = context.Update.CallbackData ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(context.Update.CallbackId))
            await context.Bot.AnswerCallbackAsync(context.Update.CallbackId, ct);

        if (string.Equals(data, UiStrings.CallbackData.NavBack, StringComparison.Ordinal))
        {
            await OnBackAsync(context, ct);
            return;
        }

        await context.Bot.SendTextAsync(context.Update.ChatId, UiStrings.Errors.UnknownCmd, ParseMode.None, null, ct);
    }

    public Task OnBackAsync(UpdateContext context, CancellationToken ct) => EnterAsync(context, ct);
}
