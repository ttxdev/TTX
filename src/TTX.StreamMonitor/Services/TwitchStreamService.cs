using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TTX.Commands.Creators.UpdateStreamStatus;
using TTX.Models;
using TTX.ValueObjects;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace TTX.StreamMonitor.Services;

public class TwitchStreamService
{
    private readonly TwitchAPI TwitchApi;
    private readonly IServiceProvider Services;
    private readonly ILogger Logger;
    private readonly LiveStreamMonitorService StreamMonitor;
    private readonly Dictionary<Slug, Creator> Creators = [];

    public TwitchStreamService(IServiceProvider services, ILogger logger, string clientId, string clientSecret)
    {
        Services = services;
        Logger = logger;

        TwitchApi = new TwitchAPI();
        TwitchApi.Settings.ClientId = clientId;
        TwitchApi.Settings.Secret = clientSecret;

        StreamMonitor = new LiveStreamMonitorService(TwitchApi);
        StreamMonitor.OnServiceStarted += async (_, e) => await OnReady();
        StreamMonitor.OnStreamOnline += async (_, e) => await OnStreamOnline(e);
        StreamMonitor.OnStreamOffline += async (_, e) => await OnStreamOffline(e);
    }

    public void AddCreator(Creator creator) => Creators[creator.Slug] = creator;
    public void RemoveCreator(Creator creator) => Creators.Remove(creator.Slug);

    public Task Start(CancellationToken cancellationToken)
    {
        StreamMonitor.SetChannelsByName(Creators.Values.Select(c => c.Slug.Value).ToList());
        StreamMonitor.Start();
        return Task.Run(() =>
        {
            while (!cancellationToken.IsCancellationRequested) { }
        }, cancellationToken);
    }

    private async Task OnReady()
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
            {
                if (liveStreams.TryGetValue(creator.Slug, out var stream))
                {
                    await UpdateStatus(new UpdateStreamStatusCommand
                    {
                        CreatorSlug = creator.Slug,
                        IsLive = true,
                        At = stream.StartedAt,
                    });
                }
                else if (creator.StreamStatus.IsLive)
                {
                    await UpdateStatus(new UpdateStreamStatusCommand
                    {
                        CreatorSlug = creator.Slug,
                        IsLive = false,
                        At = DateTime.UtcNow,
                    });
                }
            }
        }

        Logger.LogInformation("Listening...");
    }

    private async Task OnStreamOnline(OnStreamOnlineArgs e)
    {
        var creator = Creators.Values.FirstOrDefault(c =>
            string.Equals(c.Slug, e.Stream.UserLogin, StringComparison.OrdinalIgnoreCase));
        if (creator is null) return;

        await UpdateStatus(new UpdateStreamStatusCommand
        {
            CreatorSlug = creator.Slug,
            IsLive = true,
            At = e.Stream.StartedAt,
        });
    }

    private async Task OnStreamOffline(OnStreamOfflineArgs e)
    {
        var creator = Creators.Values.FirstOrDefault(c =>
            string.Equals(c.Slug, e.Channel, StringComparison.OrdinalIgnoreCase));

        if (creator is null) return;

        await UpdateStatus(new UpdateStreamStatusCommand
        {
            CreatorSlug = creator.Slug,
            IsLive = false,
            At = DateTime.UtcNow,
        });
    }

    private async Task UpdateStatus(UpdateStreamStatusCommand cmd)
    {
        using AsyncServiceScope scope = Services.CreateAsyncScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(cmd);
        if (cmd.IsLive)
        {
            Logger.LogInformation("{CreatorSlug} is live", cmd.CreatorSlug);
        }
        else
        {
            Logger.LogInformation("{CreatorSlug} stopped streaming", cmd.CreatorSlug);
        }
    }

    private static IEnumerable<List<T>> SplitList<T>(List<T> items, int nSize = 30)
    {
        for (int i = 0; i < items.Count; i += nSize)
        {
            yield return items.GetRange(i, Math.Min(nSize, items.Count - i));
        }
    }
}
