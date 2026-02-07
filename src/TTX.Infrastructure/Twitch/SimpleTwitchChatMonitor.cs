using Microsoft.Extensions.Logging;
using TTX.App.Jobs.CreatorValues;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;

namespace TTX.Infrastructure.Twitch;

public sealed class SimpleTwitchChatMonitor : IChatMonitorAdapter
{
    private readonly TwitchClient _twitch = new();
    private readonly Dictionary<Slug, ModelId> _creators = [];
    private readonly ILogger<SimpleTwitchChatMonitor> _logger;

    public event EventHandler<NetChangeEvent>? OnNetChange;

    public SimpleTwitchChatMonitor(ILogger<SimpleTwitchChatMonitor> logger)
    {
        _logger = logger;
        _twitch.Initialize(new ConnectionCredentials());
        _twitch.OnMessageReceived += OnMessageReceived;
        _twitch.OnConnected += OnConnected;
        _twitch.OnError += OnError;
    }

    private Task OnError(object? sender, OnErrorEventArgs e)
    {
        _logger.LogError(e.Exception, "Twitch error");
        return Task.CompletedTask;
    }

    public async Task Start(CancellationToken stoppingToken = default)
    {
        await _twitch.ConnectAsync();
        _logger.LogInformation("Started.");
        await Task.Delay(-1, stoppingToken);
    }

    public void SetCreators(IEnumerable<Creator> creators)
    {
        foreach (Creator creator in creators)
        {
            _creators.Add(creator.Slug, creator.Id);
        }
    }

    private async Task OnConnected(object? sender, TwitchLib.Client.Events.OnConnectedEventArgs e)
    {
        foreach (Slug username in _creators.Keys)
        {
            await _twitch.JoinChannelAsync(username.Value.ToLower());
        }
    }


    public async Task OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        int value = GetValue(e.ChatMessage.Message);
        if (value == 0)
        {
            return;
        }

        ModelId creatorId = _creators[e.ChatMessage.Channel.ToLower()];
        OnNetChange?.Invoke(this, new NetChangeEvent(creatorId, value));
    }

    private static int GetValue(string content)
    {
        if (content.Contains("+2"))
        {
            return 2;
        }

        if (content.Contains("-2"))
        {
            return -2;
        }

        return 0;
    }
}
