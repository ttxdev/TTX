using TTX.App.Interfaces.Chat;

namespace TTX.Infrastructure.Twitch.Chat;

public sealed class TwitchChatAdapter : IChatMonitorAdapter, IDisposable
{
    public event EventHandler<Message>? OnMessage;
    private readonly BotContainer _bots;

    public TwitchChatAdapter(BotContainer bots)
    {
        _bots = bots;
        _bots.OnMessage += Bot_OnMessage;
    }

    private void Bot_OnMessage(object? sender, Message e)
    {
        OnMessage?.Invoke(this, e);
    }

    public async Task Start(IEnumerable<string> channels, CancellationToken stoppingToken = default)
    {
        foreach (string channel in channels)
        {
            await _bots.AddChannel(channel);
        }

        await _bots.Start();
    }

    public async Task<bool> Add(string channel)
    {
        await _bots.AddChannel(channel);
        return true;
    }

    public async Task<bool> Remove(string channel)
    {
        await _bots.RemoveChannel(channel);
        return true;
    }

    public void Dispose()
    {
        _bots.OnMessage -= Bot_OnMessage;
    }
}
