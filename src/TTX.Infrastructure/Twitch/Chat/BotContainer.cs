using Microsoft.Extensions.DependencyInjection;
using TTX.Domain.ValueObjects;
using TwitchLib.Client.Models;
using Message = TTX.App.Interfaces.Chat.Message;

namespace TTX.Infrastructure.Twitch.Chat;

public class BotContainer(IServiceScopeFactory _scopeFactory)
{
    public event EventHandler<Message>? OnMessage;
    private readonly List<TwitchBot> _bots = [];
    public readonly List<Task> RunTasks = [];
    private const int CHUNK = 100;
    private ConnectionCredentials _credentials = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public int BotCount => _bots.Select(b => b.IsConnected).Count();
    public int ChannelCount => _bots.Sum(b => b.ChannelCount);

    public bool HasChannel(string channel)
    {
        return _bots.Any(b => b.HasChannel(channel));
    }

    public void SetCredentials(ConnectionCredentials credentials)
    {
        _credentials = credentials;
    }

    public async Task AddChannel(string channel)
    {
        await _lock.WaitAsync();
        try
        {
            TwitchBot? bot = _bots.LastOrDefault();
            if (bot is not null && bot.ChannelCount < CHUNK)
            {
                await bot.AddChannel(channel);
                return;
            }

            bot = CreateBot();
            _bots.Add(bot);

            await bot.Start();
            await bot.AddChannel(channel);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task Setup(IEnumerable<string> channels)
    {
        await _lock.WaitAsync();
        try
        {
            await Task.WhenAll(
            channels.Chunk(CHUNK).Select(channels =>
                {
                    TwitchBot bot = CreateBot();
                    _bots.Add(bot);
                    return Task.WhenAll(channels.Select(c => bot.AddChannel(c)));
                })
            );
        }
        finally
        {
            _lock.Release();
        }
    }

    public Task Start()
    {
        return Task.WhenAll(_bots.Select(b => b.Start()));
    }

    public async Task<bool> RemoveChannel(string channel)
    {
        await _lock.WaitAsync();
        try
        {
            TwitchBot? bot = FindBotByCreator(channel);
            if (bot is null)
            {
                return false;
            }

            await bot.RemoveChannel(channel);
            if (bot.ChannelCount == 0)
            {
                await bot.Stop();
                _bots.Remove(bot);
            }

            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    private TwitchBot? FindBotByCreator(Slug creator)
    {
        return _bots.FirstOrDefault(b => b.HasChannel(creator));
    }

    private TwitchBot CreateBot()
    {
        IServiceScope scope = _scopeFactory.CreateScope();
        TwitchBot bot = scope.ServiceProvider.GetRequiredService<TwitchBot>();
        bot.SetCredentials(_credentials);
        bot.OnMessage += OnMessage;

        return bot;
    }
}
