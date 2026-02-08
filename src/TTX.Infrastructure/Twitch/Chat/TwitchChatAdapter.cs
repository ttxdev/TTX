using TTX.App.Interfaces.Chat;

namespace TTX.Infrastructure.Twitch.Chat;

public sealed class TwitchChatAdapter : IChatMonitorAdapter
{
    public event EventHandler<Message>? OnMessage;
    private readonly BotContainer _botContainer;

    public TwitchChatAdapter(BotContainer botContainer)
    {
        _botContainer = botContainer;
        _botContainer.OnMessage += OnMessage;
    }

    public async Task Start(IEnumerable<string> channels, CancellationToken stoppingToken = default)
    {
        await _botContainer.Setup(channels);
        await _botContainer.Start();
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
