using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TTX.Commands.Creators.UpdateStreamStatus;
using TTX.Models;
using TTX.ValueObjects;
using TwitchLib.Api;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;

namespace TTX.StreamMonitor.Services;

public class TwitchStreamService
{
    private readonly Dictionary<TwitchId, Creator> Creators = [];
    private readonly ILogger Logger;
    private readonly IServiceProvider Services;
    private readonly TwitchAPI TwitchApi;
    private readonly TwitchPubSub Client;

    public TwitchStreamService(IServiceProvider services, ILogger logger, string clientId, string clientSecret)
    {
        Services = services;
        Logger = logger;

        TwitchApi = new TwitchAPI();
        TwitchApi.Settings.ClientId = clientId;
        TwitchApi.Settings.Secret = clientSecret;

        Client = new TwitchPubSub();
        Client.OnPubSubServiceConnected += (_, e) => OnReady();
        Client.OnStreamUp += (_, e) => OnStreamOnline(e);
        Client.OnStreamDown += (_, e) => OnStreamOffline(e);
    }

    public void AddCreator(Creator creator)
    {
        Creators[creator.TwitchId] = creator;
    }

    public void RemoveCreator(Creator creator)
    {
        Creators.Remove(creator.TwitchId);
    }

    public Task Start(CancellationToken cancellationToken)
    {
        foreach (Creator creator in Creators.Values)
        {
            Client.ListenToVideoPlayback(creator.Slug.Value);
        }

        Client.Connect();
        return Task.Run(() =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
            }
        }, cancellationToken);
    }

    private async void OnReady()
    {
        Logger.LogInformation("Ready");
        var creatorSlugs = Creators.Values.Select(c => c.Slug.Value).ToList();
        foreach (var chunk in SplitList(creatorSlugs, 100))
        {
            var streams = await TwitchApi.Helix.Streams.GetStreamsAsync(userLogins: chunk);
            var liveStreams = streams.Streams.ToDictionary(
                s => s.UserLogin.ToLower(),
                s => s
            );

            foreach (var creator in Creators.Values.Where(c => chunk.Contains(c.Slug)))
                if (liveStreams.TryGetValue(creator.Slug, out var stream))
                    await UpdateStatus(new UpdateStreamStatusCommand
                    {
                        CreatorSlug = creator.Slug,
                        IsLive = true,
                        At = stream.StartedAt
                    });
                else if (creator.StreamStatus.IsLive)
                    await UpdateStatus(new UpdateStreamStatusCommand
                    {
                        CreatorSlug = creator.Slug,
                        IsLive = false,
                        At = DateTime.UtcNow
                    });
        }

        Logger.LogInformation("Listening...");
    }

    private async void OnStreamOnline(OnStreamUpArgs e)
    {
        var creator = Creators[e.ChannelId];
        if (creator is null) return;

        await UpdateStatus(new UpdateStreamStatusCommand
        {
            CreatorSlug = creator.Slug,
            IsLive = true,
            At = DateTime.UtcNow
        });
    }

    private async void OnStreamOffline(OnStreamDownArgs e)
    {
        var creator = Creators[e.ChannelId];
        if (creator is null) return;

        await UpdateStatus(new UpdateStreamStatusCommand
        {
            CreatorSlug = creator.Slug,
            IsLive = false,
            At = DateTime.UtcNow
        });
    }

    private async Task UpdateStatus(UpdateStreamStatusCommand cmd)
    {
        using var scope = Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(cmd);
        if (cmd.IsLive)
            Logger.LogInformation("{CreatorSlug} is live", cmd.CreatorSlug);
        else
            Logger.LogInformation("{CreatorSlug} stopped streaming", cmd.CreatorSlug);
    }

    private static IEnumerable<List<T>> SplitList<T>(List<T> items, int nSize = 30)
    {
        for (var i = 0; i < items.Count; i += nSize) yield return items.GetRange(i, Math.Min(nSize, items.Count - i));
    }
}
