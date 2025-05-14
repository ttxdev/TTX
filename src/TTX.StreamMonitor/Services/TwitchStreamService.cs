using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TTX.Commands.Creators.UpdateStreamStatus;
using TTX.Models;
using TTX.ValueObjects;
using TwitchLib.Api;

namespace TTX.StreamMonitor.Services;

public class TwitchStreamService
{
    private readonly Dictionary<TwitchId, Creator> _creators = [];
    private readonly ILogger _logger;
    private readonly IServiceProvider _services;
    private readonly TwitchAPI _twitch;

    public TwitchStreamService(IServiceProvider services, ILogger logger, string clientId, string clientSecret)
    {
        _services = services;
        _logger = logger;

        _twitch = new TwitchAPI();
        _twitch.Settings.ClientId = clientId;
        _twitch.Settings.Secret = clientSecret;
    }

    public void AddCreator(Creator creator)
    {
        _creators[creator.TwitchId] = creator;
    }

    public void RemoveCreator(Creator creator)
    {
        _creators.Remove(creator.TwitchId);
    }

    public Task Start(CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Check();
                await Task.Delay(300_000);
            }
        }, cancellationToken);
    }

    private async Task Check()
    {
        var creatorSlugs = _creators.Values.Select(c => c.Slug.Value).ToList();
        foreach (var chunk in SplitList(creatorSlugs, 100))
        {
            var streams = await _twitch.Helix.Streams.GetStreamsAsync(userLogins: chunk);
            var liveStreams = streams.Streams.ToDictionary(
                s => s.UserLogin.ToLower(),
                s => s
            );

            foreach (var creator in _creators.Values.Where(c => chunk.Contains(c.Slug)))
                if (liveStreams.TryGetValue(creator.Slug, out var stream))
                    await UpdateStatus(new UpdateStreamStatusCommand
                    {
                        Username = creator.Slug,
                        IsLive = true,
                        At = stream.StartedAt
                    });
                else if (creator.StreamStatus.IsLive)
                    await UpdateStatus(new UpdateStreamStatusCommand
                    {
                        Username = creator.Slug,
                        IsLive = false,
                        At = DateTime.UtcNow
                    });
        }
    }

    private async Task UpdateStatus(UpdateStreamStatusCommand cmd)
    {
        await using var scope = _services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(cmd);
        _logger.LogInformation(cmd.IsLive ? "{CreatorSlug} is live" : "{CreatorSlug} stopped streaming", cmd.Username);
    }

    private static IEnumerable<List<T>> SplitList<T>(List<T> items, int nSize = 30)
    {
        for (var i = 0; i < items.Count; i += nSize) yield return items.GetRange(i, Math.Min(nSize, items.Count - i));
    }
}
