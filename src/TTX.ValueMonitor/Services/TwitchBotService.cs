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
    private readonly TwitchClient Client = new();
    private readonly HashSet<Slug> Creators = [];
    private readonly ILogger Logger;
    private readonly IServiceProvider Services;

    public TwitchBotService(IServiceProvider services, ILogger logger)
    {
        Services = services;
        Logger = logger;
        Client.Initialize(new ConnectionCredentials("justinfan2425", ""));
        Client.OnMessageReceived += async (_, e) => { await OnMessage(e.ChatMessage.Channel, e.ChatMessage.Message); };
        Client.OnConnected += (_, e) =>
        {
            foreach (var username in Creators)
            {
                Logger.LogInformation("Listening to {username}", username);
                Client.JoinChannel(username);
            }
        };
    }

    public Task Start(CancellationToken token)
    {
        Logger.LogInformation("Started.");
        Client.Connect();
        return Task.Run(() =>
        {
            while (!token.IsCancellationRequested)
            {
            }
        }, token);
    }

    public void AddCreator(Creator creator)
    {
        Creators.Add(creator.Slug);
    }

    public void RemoveCreator(Creator creator)
    {
        Creators.Remove(creator.Slug);
    }

    private async Task OnMessage(Slug creatorSlug, string content)
    {
        var value = GetValue(content);
        if (value == 0) return;

        using var scope = Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        try
        {
            await sender.Send(new RecordNetChangeCommand
            {
                CreatorSlug = creatorSlug,
                NetChange = value
            });
            if (value > 0)
                Logger.LogInformation("{creatorSlug} gained {value}", creatorSlug, value);
            else
                Logger.LogInformation("{creatorSlug} lost {value}", creatorSlug, value);
        }
        catch (DomainException ex)
        {
            Logger.LogError("Fail to store value, {message}", ex.Message);
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