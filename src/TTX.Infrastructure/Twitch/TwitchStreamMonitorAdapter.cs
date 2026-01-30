using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TTX.App.Jobs.Streams;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;
using TTX.Infrastructure.Options;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;

namespace TTX.Infrastructure.Twitch;

public class TwitchStreamMonitorAdapter(
    IOptions<TwitchOAuthOptions> _twitchOptions,
    IOptions<TwitchStreamMonitorOptions> _streamMonitorOptions
) : IStreamMonitorAdapter
{
    public event EventHandler<StreamUpdateEvent>? StreamStatusUpdated;

    private readonly Dictionary<PlatformId, Creator> _creators = [];
    private readonly TwitchAPI _twitch = new()
    {
        Settings = {
            ClientId = _twitchOptions.Value.ClientId,
            Secret = _twitchOptions.Value.ClientSecret
        }
    };

    public async Task Start(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Check();
            await Task.Delay(_streamMonitorOptions.Value.Delay, cancellationToken);
        }
    }

    public void SetCreators(IEnumerable<Creator> creators)
    {
        foreach (Creator creator in creators)
        {
            _creators[creator.PlatformId] = creator;
        }
    }

    public bool RemoveCreator(ModelId creatorId)
    {
        Creator? creator = _creators.Select(kv => kv.Value).FirstOrDefault(c => c.Id == creatorId);
        if (creator is null)
        {
            return false;
        }

        return _creators.Remove(creator.PlatformId);
    }

    private async Task Check()
    {
        List<string> creatorSlugs = [.. _creators.Values.Select(c => c.Slug.Value)];
        foreach (List<string> chunk in SplitList(creatorSlugs, 100))
        {
            GetStreamsResponse streams = await _twitch.Helix.Streams.GetStreamsAsync(userLogins: chunk);
            var liveStreams = streams.Streams.ToDictionary(
                s => s.UserLogin.ToLower(),
                s => s
            );

            foreach (Creator? creator in _creators.Values.Where(c => chunk.Contains(c.Slug)))
            {
                if (liveStreams.TryGetValue(creator.Slug, out var stream))
                {
                    StreamStatusUpdated?.Invoke(this, new()
                    {
                        CreatorId = creator.Id,
                        IsLive = true,
                        At = stream.StartedAt
                    });
                }
                else if (creator.StreamStatus.IsLive)
                {
                    StreamStatusUpdated?.Invoke(this, new()
                    {
                        CreatorId = creator.Id,
                        IsLive = false,
                        At = DateTime.UtcNow
                    });
                }
            }
        }
    }

    private static IEnumerable<List<T>> SplitList<T>(List<T> items, int nSize = 30)
    {
        for (int i = 0; i < items.Count; i += nSize) yield return items.GetRange(i, Math.Min(nSize, items.Count - i));
    }

}
