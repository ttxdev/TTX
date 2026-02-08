using Microsoft.Extensions.Logging;
using TTX.App.Interfaces.Chat;

namespace TTX.Infrastructure.Twitch.Chat;

public sealed class TwitchChatAdapter : IChatMonitorAdapter
{
    public event EventHandler<Message>? OnMessage;
    private readonly BotContainer _botContainer;
    private readonly ILogger<TwitchChatAdapter> _logger;

    public TwitchChatAdapter(BotContainer botContainer, ILogger<TwitchChatAdapter> logger)
    {
        _logger = logger;
        _botContainer = botContainer;
        _botContainer.OnMessage += OnMessage;
    }

    public async Task Start(IEnumerable<string> channels, CancellationToken stoppingToken = default)
    {
        await _botContainer.Setup(channels);
        await _botContainer.Start();

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Bot count: {BotCount}, Channel count: {ChannelCount}",
                    _botContainer.BotCount,
                    _botContainer.ChannelCount
                );
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    public async Task<bool> Add(string channel)
    {
        if (_botContainer.HasChannel(channel))
        {
            return true;
        }

        await _botContainer.AddChannel(channel);
        return true;
    }

    public async Task<bool> Remove(string channel)
    {
        if (!_botContainer.HasChannel(channel))
        {
            return false;
        }

        await _botContainer.RemoveChannel(channel);
        return true;
    }
}
