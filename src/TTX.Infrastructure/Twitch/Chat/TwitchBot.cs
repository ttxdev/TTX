using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TTX.Domain.ValueObjects;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using Message = TTX.App.Interfaces.Chat.Message;

namespace TTX.Infrastructure.Twitch.Chat;

public class TwitchBot
{
    public bool IsConnected => _client.IsConnected;
    public int ChannelCount => _client.JoinedChannels.Count + _joinQueue.Count;
    public event EventHandler<Message> OnMessage = null!;
    private readonly ILogger<TwitchBot> _logger;
    private readonly TwitchClient _client;
    private readonly ConcurrentQueue<string> _joinQueue = [];

    public TwitchBot(ILogger<TwitchBot> logger, ILoggerFactory loggerFactory)
    {
        _logger = logger;

        _client = new(loggerFactory: loggerFactory);
        _client.OnNoPermissionError += OnNoPermissionError;
        _client.OnUnaccountedFor += OnUnaccountedFor;
        _client.OnMessageReceived += OnTwitchMessage;
        _client.OnRateLimit += OnRateLimit;
        _client.OnConnectionError += OnConnectionError;
        _client.OnFailureToReceiveJoinConfirmation += OnFailureToJoin;
        _client.OnError += OnError;
        _client.OnConnected += OnConnected;
        _client.OnJoinedChannel += OnJoinedChannel;
        _client.Initialize(new ConnectionCredentials(new Capabilities(membership: true)));
    }

    private Task OnNoPermissionError(object? sender, NoticeEventArgs e)
    {
        if (_logger.IsEnabled(LogLevel.Warning))
        {
            _logger.LogWarning("No permission error received {message}", e.Message);
        }

        return Task.CompletedTask;
    }

    private Task OnUnaccountedFor(object? sender, OnUnaccountedForArgs e)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Unaccounted for event received {message}", e.RawIRC);
        }

        return Task.CompletedTask;
    }

    private Task OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Joined {channel}", e.Channel);
        }

        return Task.CompletedTask;
    }

    public bool HasChannel(string channel) =>
        _client.JoinedChannels.Any(c => c.Channel == channel)
         || _joinQueue.Any(c => c == channel);

    public Task Start() => _client.ConnectAsync();

    public Task Stop() => _client.DisconnectAsync();

    public async Task AddChannel(string channel)
    {
        _joinQueue.Enqueue(channel);

        if (_client.IsConnected)
        {
            await JoinAll();
        }
    }

    public Task RemoveChannel(string channel)
    {
        return _client.LeaveChannelAsync(channel);
    }

    private Task OnConnectionError(object? sender, OnConnectionErrorArgs e)
    {
        if (_logger.IsEnabled(LogLevel.Error))
        {
            _logger.LogError("Connection error {message}", e.Error.Message);
        }

        return Task.CompletedTask;
    }

    private Task OnRateLimit(object? sender, NoticeEventArgs e)
    {
        if (_logger.IsEnabled(LogLevel.Warning))
        {
            _logger.LogWarning("Rate limit exceeded {desc}", e.Message);
        }

        return Task.CompletedTask;
    }

    private Task OnConnected(object? sender, OnConnectedEventArgs e) => JoinAll();

    private Task OnError(object? sender, TwitchLib.Communication.Events.OnErrorEventArgs e)
    {
        _logger.LogError(e.Exception, "Ran into an error");
        return Task.CompletedTask;
    }

    private Task OnFailureToJoin(object? sender, OnFailureToReceiveJoinConfirmationArgs e)
    {
        if (_logger.IsEnabled(LogLevel.Warning))
        {
            _logger.LogWarning("Failed to join {channel} because {details}", e.Exception.Channel, e.Exception);
        }

        return Task.CompletedTask;
    }

    private Task OnTwitchMessage(object? _, OnMessageReceivedArgs @event)
    {
        Slug slug = @event.ChatMessage.Channel;
        string content = @event.ChatMessage.Message;
        OnMessage.Invoke(this, new Message(slug, content));
        return Task.CompletedTask;
    }

    private async Task JoinAll()
    {
        while (!_joinQueue.IsEmpty)
        {
            if (!_joinQueue.TryDequeue(out string? channel))
            {
                continue;
            }

            await _client.JoinChannelAsync(channel!);
            await Task.Delay(5_000);
        }
    }
}
