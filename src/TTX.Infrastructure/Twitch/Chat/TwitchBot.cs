using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using Message = TTX.App.Interfaces.Chat.Message;

namespace TTX.Infrastructure.Twitch.Chat;

public sealed class TwitchBot : IAsyncDisposable
{
    public bool IsConnected => _client.IsConnected;
    public event EventHandler<Message> OnMessage = null!;
    public int ChannelCount => _channels.Count;
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger<TwitchBot> _logger;
    private readonly TwitchClient _client;
    private readonly Channel<string> _joinChannel = Channel.CreateUnbounded<string>();
    private readonly HashSet<string> _channels = new(StringComparer.OrdinalIgnoreCase);

    public TwitchBot(ILogger<TwitchBot> logger, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        ClientOptions clientOptions = new(
            reconnectionPolicy: new ReconnectionPolicy(3_000)
        );
        _client = new TwitchClient(
            new WebSocketClient(clientOptions),
            loggerFactory: loggerFactory
        );

        _client.Initialize(new ConnectionCredentials());
        _client.OnMessageReceived += OnMessageReceived;
        _client.OnConnected += OnClientConnected;
        _client.OnReconnected += OnClientReconnected;
        _client.OnDisconnected += OnClientDisconnected;
        _client.OnConnectionError += OnConnectionError;
        _client.OnFailureToReceiveJoinConfirmation += OnFailureToReceiveJoinConfirmation;
    }

    public Task Start() => _client.ConnectAsync();

    public ValueTask AddChannel(string channel)
    {
        _channels.Add(channel);
        return _joinChannel.Writer.WriteAsync(channel);
    }

    public async Task RemoveChannel(string channel)
    {
        _channels.Remove(channel);
        await _client.LeaveChannelAsync(channel);
    }

    private Task OnMessageReceived(object? _, OnMessageReceivedArgs e)
    {
        OnMessage?.Invoke(this, new Message(e.ChatMessage.Channel, e.ChatMessage.Message));
        return Task.CompletedTask;
    }

    private Task OnFailureToReceiveJoinConfirmation(object? _, OnFailureToReceiveJoinConfirmationArgs e)
    {
        if (_logger.IsEnabled(LogLevel.Warning))
        {
            _logger.LogWarning("Failed to join {Channel}: {Error}", e.Exception.Channel, e.Exception.Details);
        }
        _joinChannel.Writer.TryWrite(e.Exception.Channel);

        return Task.CompletedTask;
    }

    private Task OnClientConnected(object? _, OnConnectedEventArgs e)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("TwitchBot connected as {Username}", e.BotUsername);
        }

        _ = DrainJoinQueue();
        return Task.CompletedTask;
    }

    private Task OnClientReconnected(object? _, OnConnectedEventArgs e)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("TwitchBot reconnected as {Username}", e.BotUsername);
        }

        foreach (string channel in _channels)
        {
            _joinChannel.Writer.TryWrite(channel);
        }

        return Task.CompletedTask;
    }

    private Task OnClientDisconnected(object? _, OnDisconnectedArgs e)
    {
        if (_logger.IsEnabled(LogLevel.Warning))
        {
            _logger.LogWarning("TwitchBot disconnected");
        }

        return Task.CompletedTask;
    }

    private Task OnConnectionError(object? _, OnConnectionErrorArgs e)
    {
        if (_logger.IsEnabled(LogLevel.Error))
        {
            _logger.LogError("TwitchBot connection error: {Message}", e.Error.Message);
        }

        return Task.CompletedTask;
    }

    private async Task DrainJoinQueue()
    {
        while (!_cts.Token.IsCancellationRequested && await _joinChannel.Reader.WaitToReadAsync())
        {
            while (_joinChannel.Reader.TryRead(out string? channel))
            {
                await _client.JoinChannelAsync(channel);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _client.OnMessageReceived -= OnMessageReceived;
        _client.OnConnected -= OnClientConnected;
        _client.OnReconnected -= OnClientReconnected;
        _client.OnDisconnected -= OnClientDisconnected;
        _client.OnConnectionError -= OnConnectionError;
        _client.OnFailureToReceiveJoinConfirmation -= OnFailureToReceiveJoinConfirmation;
        await _cts.CancelAsync();
        await _client.DisconnectAsync();
    }
}
