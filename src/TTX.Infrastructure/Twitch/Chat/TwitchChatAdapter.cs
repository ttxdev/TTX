using Microsoft.Extensions.Options;
using TTX.App.Interfaces.Chat;
using TTX.Infrastructure.Options;
using TwitchLib.Client.Models;

namespace TTX.Infrastructure.Twitch.Chat;

public sealed class TwitchChatAdapter : IChatMonitorAdapter
{
    public event EventHandler<Message>? OnMessage;
    private readonly BotContainer _botContainer;

    public TwitchChatAdapter(BotContainer botContainer, IOptions<TwitchChatOptions> options)
    {
        _botContainer = botContainer;
        _botContainer.OnMessage += OnMessage;

        if (options.Value.Username.Length > 0)
        {
            _botContainer.SetCredentials(new ConnectionCredentials(options.Value.Username, options.Value.Token));
        }
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
