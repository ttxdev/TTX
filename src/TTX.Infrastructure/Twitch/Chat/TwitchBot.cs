using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using Message = TTX.App.Interfaces.Chat.Message;

namespace TTX.Infrastructure.Twitch.Chat;

public sealed class TwitchBot : IAsyncDisposable
{
    public bool IsConnected => _client.IsConnected;
    public event EventHandler<Message> OnMessage = null!;
    public int ChannelCount => _client.JoinedChannels.Count;
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger<TwitchBot> _logger;
    private readonly TwitchClient _client;
    private readonly Channel<string> _joinChannel = Channel.CreateUnbounded<string>();

    public TwitchBot(ILogger<TwitchBot> logger, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _client = new TwitchClient(loggerFactory: loggerFactory);

        _client.Initialize(new ConnectionCredentials());
        _client.OnMessageReceived += OnMessageReceived;
        _client.OnConnected += async (_, _) => { _ = OnConnected(); };
        _client.OnFailureToReceiveJoinConfirmation += OnFailureToReceiveJoinConfirmation;
    }

    public Task Start() => _client.ConnectAsync();

    public ValueTask AddChannel(string channel) => _joinChannel.Writer.WriteAsync(channel);

    public Task RemoveChannel(string channel) => _client.LeaveChannelAsync(channel);

    private Task OnMessageReceived(object? _, OnMessageReceivedArgs e)
    {
        OnMessage?.Invoke(this, new Message(e.ChatMessage.Channel, e.ChatMessage.Message));
        return Task.CompletedTask;
    }

    private Task OnFailureToReceiveJoinConfirmation(object? _, OnFailureToReceiveJoinConfirmationArgs e)
    {
        if (_logger.IsEnabled(LogLevel.Warning))
        {
            _logger.LogWarning("Failed to join {channel}: {error}", e.Exception.Channel, e.Exception.Details);
        }

        _joinChannel.Writer.TryWrite(e.Exception.Channel);
        return Task.CompletedTask;
    }

    private async Task OnConnected()
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
        _client.OnFailureToReceiveJoinConfirmation -= OnFailureToReceiveJoinConfirmation;
        await _cts.CancelAsync();
        await _client.DisconnectAsync();
    }
}
