using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReminderAssistantBot.Telegram.SceneEngine;
using Xunit;

namespace ReminderAssistantBot.Telegram.UnitTests;

public sealed class SceneRouterTests
{
    [Fact]
    public async Task RouteAsync_Message_CallsOnMessage()
    {
        FakeStateCache stateCache = new("main");
        TrackingScene scene = new();
        FakeSceneRegistry registry = new(scene);
        IBotClient botClient = new FakeBotClient();
        ILogger logger = NullLogger.Instance;
        BotUpdate update = new(UpdateKind.Message, 42, "hello", null, null);
        UpdateContext context = new(logger, stateCache, botClient, update);

        await SceneRouter.RouteAsync(context, registry, CancellationToken.None);

        Assert.Equal(42, stateCache.LastUserId);
        Assert.Equal("main", registry.LastStateKey);
        Assert.Equal(1, scene.MessageCalls);
        Assert.Equal(0, scene.CallbackCalls);
        Assert.Equal(0, scene.EnterCalls);
        Assert.Equal(0, scene.BackCalls);
    }

    [Fact]
    public async Task RouteAsync_Callback_CallsOnCallback()
    {
        FakeStateCache stateCache = new("main");
        TrackingScene scene = new();
        FakeSceneRegistry registry = new(scene);
        IBotClient botClient = new FakeBotClient();
        ILogger logger = NullLogger.Instance;
        BotUpdate update = new(UpdateKind.Callback, 42, null, "cb1", "data");
        UpdateContext context = new(logger, stateCache, botClient, update);

        await SceneRouter.RouteAsync(context, registry, CancellationToken.None);

        Assert.Equal(42, stateCache.LastUserId);
        Assert.Equal("main", registry.LastStateKey);
        Assert.Equal(0, scene.MessageCalls);
        Assert.Equal(1, scene.CallbackCalls);
        Assert.Equal(0, scene.EnterCalls);
        Assert.Equal(0, scene.BackCalls);
    }

    [Fact]
    public async Task RouteAsync_Other_CallsEnter()
    {
        FakeStateCache stateCache = new("main");
        TrackingScene scene = new();
        FakeSceneRegistry registry = new(scene);
        IBotClient botClient = new FakeBotClient();
        ILogger logger = NullLogger.Instance;
        BotUpdate update = new(UpdateKind.Other, 42, null, null, null);
        UpdateContext context = new(logger, stateCache, botClient, update);

        await SceneRouter.RouteAsync(context, registry, CancellationToken.None);

        Assert.Equal(42, stateCache.LastUserId);
        Assert.Equal("main", registry.LastStateKey);
        Assert.Equal(0, scene.MessageCalls);
        Assert.Equal(0, scene.CallbackCalls);
        Assert.Equal(1, scene.EnterCalls);
        Assert.Equal(0, scene.BackCalls);
    }

    [Fact]
    public async Task RouteAsync_UsesStateKeyFromCache()
    {
        FakeStateCache stateCache = new("scene_x");
        TrackingScene scene = new();
        FakeSceneRegistry registry = new(scene);
        IBotClient botClient = new FakeBotClient();
        ILogger logger = NullLogger.Instance;
        BotUpdate update = new(UpdateKind.Message, 10, "hi", null, null);
        UpdateContext context = new(logger, stateCache, botClient, update);

        await SceneRouter.RouteAsync(context, registry, CancellationToken.None);

        Assert.Equal("scene_x", registry.LastStateKey);
    }

    [Fact]
    public async Task RouteAsync_Message_DoesNotCallEnterOrCallback()
    {
        FakeStateCache stateCache = new("main");
        TrackingScene scene = new();
        FakeSceneRegistry registry = new(scene);
        IBotClient botClient = new FakeBotClient();
        ILogger logger = NullLogger.Instance;
        BotUpdate update = new(UpdateKind.Message, 7, "hey", null, null);
        UpdateContext context = new(logger, stateCache, botClient, update);

        await SceneRouter.RouteAsync(context, registry, CancellationToken.None);

        Assert.Equal(0, scene.EnterCalls);
        Assert.Equal(0, scene.CallbackCalls);
    }

    [Fact]
    public async Task RouteAsync_Callback_DoesNotCallEnterOrMessage()
    {
        FakeStateCache stateCache = new("main");
        TrackingScene scene = new();
        FakeSceneRegistry registry = new(scene);
        IBotClient botClient = new FakeBotClient();
        ILogger logger = NullLogger.Instance;
        BotUpdate update = new(UpdateKind.Callback, 11, null, "cb", "x");
        UpdateContext context = new(logger, stateCache, botClient, update);

        await SceneRouter.RouteAsync(context, registry, CancellationToken.None);

        Assert.Equal(0, scene.EnterCalls);
        Assert.Equal(0, scene.MessageCalls);
    }

    private sealed class FakeStateCache : IStateCache
    {
        private readonly string _stateKey;

        public FakeStateCache(string stateKey) => _stateKey = stateKey;

        public long LastUserId { get; private set; }

        public Task<string> GetStateAsync(long userId)
        {
            LastUserId = userId;
            return Task.FromResult(_stateKey);
        }

        public Task SetStateAsync(long userId, string stateKey) => Task.CompletedTask;
    }

    private sealed class FakeSceneRegistry : ISceneRegistry
    {
        private readonly IScene _scene;

        public FakeSceneRegistry(IScene scene) => _scene = scene;

        public string? LastStateKey { get; private set; }

        public IScene GetScene(string stateKey)
        {
            LastStateKey = stateKey;
            return _scene;
        }

        public Task NavigateBackAsync(UpdateContext context, string fallbackKey, CancellationToken ct) =>
            throw new InvalidOperationException("Navigation is not expected in SceneRouter tests");

        public Task NavigateForwardAsync(UpdateContext context, string nextKey, CancellationToken ct) =>
            throw new InvalidOperationException("Navigation is not expected in SceneRouter tests");
    }

    private sealed class TrackingScene : IScene
    {
        public int EnterCalls { get; private set; }
        public int MessageCalls { get; private set; }
        public int CallbackCalls { get; private set; }
        public int BackCalls { get; private set; }

        public Task EnterAsync(UpdateContext context, CancellationToken ct)
        {
            EnterCalls++;
            return Task.CompletedTask;
        }

        public Task OnMessageAsync(UpdateContext context, CancellationToken ct)
        {
            MessageCalls++;
            return Task.CompletedTask;
        }

        public Task OnCallbackAsync(UpdateContext context, CancellationToken ct)
        {
            CallbackCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeBotClient : IBotClient
    {
        public Task SendTextAsync(long userId, string text, ParseMode parseMode, BotInlineKeyboard? keyboard, CancellationToken ct) =>
            Task.CompletedTask;

        public Task AnswerCallbackAsync(string callbackId, CancellationToken ct) => Task.CompletedTask;
    }
}
