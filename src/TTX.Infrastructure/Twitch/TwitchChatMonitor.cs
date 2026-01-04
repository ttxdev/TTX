using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TTX.App.Jobs.CreatorValues;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;
using TTX.Infrastructure.Options;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;

namespace TTX.Infrastructure.Twitch;

public sealed class TwitchChatMonitor : IChatMonitorAdapter
{
    private readonly TwitchClient _twitch = new();
    private readonly Dictionary<Slug, ModelId> _creators = [];
    private readonly ILogger<TwitchChatMonitor> _logger;

    public event EventHandler<MessageEvent>? OnMessage;

    public TwitchChatMonitor(IOptions<TwitchChatMonitorOptions> options, ILogger<TwitchChatMonitor> logger)
    {
        _logger = logger;
        _twitch.Initialize(new ConnectionCredentials(options.Value.Username, options.Value.Password));
        _twitch.OnMessageReceived += OnMessageReceived;
        _twitch.OnConnected += OnConnected;
        _twitch.OnError += OnError;
        _twitch.OnLog += OnLog;
    }

    private void OnLog(object? sender, OnLogArgs e)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Twitch: {data}", e.Data);
        }
    }

    private void OnError(object? sender, OnErrorEventArgs e)
    {
        _logger.LogError(e.Exception, "Twitch error");
    }

    public Task Start(CancellationToken token)
    {
        _twitch.Connect();
        _logger.LogInformation("Started.");
        return Task.Run(() =>
        {
            while (!token.IsCancellationRequested)
            {
            }

            _twitch.Disconnect();
        }, token);
    }

    public void SetCreators(IEnumerable<Creator> creators)
    {
        foreach (Creator creator in creators)
        {
            _creators.Add(creator.Slug, creator.Id);
        }
    }

    private void OnConnected(object? sender, EventArgs e)
    {
        foreach (Slug username in _creators.Keys)
        {
            _twitch.JoinChannel(username.Value.ToLower());
        }
    }

    private void OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        ModelId creatorId = _creators[e.ChatMessage.Channel.ToLower()];
        OnMessage?.Invoke(this, new MessageEvent(creatorId, e.ChatMessage.Message));
    }
}
