using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TTX.Commands.Creators.RecordNetChange;
using TTX.Exceptions;
using TTX.Models;
using TTX.ValueObjects;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace TTX.ValueMonitor.Services;

public class TwitchBotService
{
    private readonly TwitchClient _twitch = new();
    private readonly HashSet<Slug> _creators = [];
    private readonly ILogger _logger;
    private readonly IServiceProvider _services;

    public TwitchBotService(IServiceProvider services, ILogger logger)
    {
        _services = services;
        _logger = logger;
        _twitch.Initialize(new ConnectionCredentials("justinfan2425", ""));
        _twitch.OnMessageReceived += async (_, e) => { await OnMessage(e.ChatMessage.Channel, e.ChatMessage.Message); };
        _twitch.OnConnected += (_, e) =>
        {
            foreach (var username in _creators)
            {
                _logger.LogInformation("Listening to {username}", username);
                _twitch.JoinChannel(username);
            }
        };
    }

    public Task Start(CancellationToken token)
    {
        _logger.LogInformation("Started.");
        _twitch.Connect();
        return Task.Run(() =>
        {
            while (!token.IsCancellationRequested)
            {
            }
        }, token);
    }

    public void AddCreator(Creator creator)
    {
        _creators.Add(creator.Slug);
    }

    public void RemoveCreator(Creator creator)
    {
        _creators.Remove(creator.Slug);
    }

    private async Task OnMessage(Slug creatorSlug, string content)
    {
        var value = GetValue(content);
        if (value == 0) return;

        using var scope = _services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        try
        {
            await sender.Send(new RecordNetChangeCommand
            {
                Username = creatorSlug,
                NetChange = value
            });
            _logger.LogInformation(value > 0 ? "{creatorSlug} gained {value}" : "{creatorSlug} lost {value}",
                creatorSlug, value);
        }
        catch (DomainException ex)
        {
            _logger.LogError("Fail to store value, {message}", ex.Message);
        }
    }

    private static int GetValue(string content)
    {
        if (content.Contains("+2"))
            return 2;
        if (content.Contains("-2"))
            return -2;

        return 0;
    }
}