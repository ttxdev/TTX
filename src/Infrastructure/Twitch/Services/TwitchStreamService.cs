using Microsoft.Extensions.Logging;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TTX.Core.Models;
using TTX.Core.Interfaces;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace TTX.Infrastructure.Twitch.Services;

public class TwitchStreamMonitor : IStreamService
{
    private readonly TwitchAPI _twitchApi;
    private readonly LiveStreamMonitorService _liveStreamMonitor;
    private readonly Dictionary<string, Creator> _creators = [];
    public event EventHandler<StreamStatusUpdate> OnStreamUpdate = delegate { };

    public TwitchStreamMonitor(string clientId, string clientSecret)
    {
        _twitchApi = new TwitchAPI();
        _twitchApi.Settings.ClientId = clientId;
        _twitchApi.Settings.Secret = clientSecret;

        _liveStreamMonitor = new LiveStreamMonitorService(_twitchApi);
        _liveStreamMonitor.OnStreamOnline += OnStreamOnline;
        _liveStreamMonitor.OnStreamOffline += OnStreamOffline;
    }

    public void AddCreator(Creator creator) => _creators[creator.Slug.ToLower()] = creator;
    public void RemoveCreator(Creator creator) => _creators.Remove(creator.Slug.ToLower());

    public async Task Start(CancellationToken cancellationToken)
    {
        _liveStreamMonitor.SetChannelsByName(_creators.Values.Select(c => c.Slug.ToLower()).ToList());

        await OnReady();
        _liveStreamMonitor.Start();
    }

    private async Task OnReady()
    {
        var creatorSlugs = _creators.Values.Select(c => c.Slug.ToLower()).ToList();
        foreach (var chunk in SplitList(creatorSlugs, 100))
        {
            var streams = await _twitchApi.Helix.Streams.GetStreamsAsync(userLogins: chunk);
            var liveStreams = streams.Streams.ToDictionary(
                s => s.UserLogin.ToLower(),
                s => s
            );

            foreach (var creator in _creators.Values.Where(c => chunk.Contains(c.Slug.ToLower())))
            {
                if (liveStreams.TryGetValue(creator.Slug.ToLower(), out var stream))
                {
                    OnStreamUpdate.Invoke(this, new StreamStatusUpdate
                    {
                        CreatorId = creator.Id,
                        IsLive = true,
                        StartedAt = stream.StartedAt,
                    });
                }
                else if (creator.StreamStatus.IsLive)
                {
                    OnStreamUpdate.Invoke(this, new StreamStatusUpdate
                    {
                        CreatorId = creator.Id,
                        IsLive = false,
                        EndedAt = DateTime.UtcNow,
                    });
                }
            }
        }

    }

    private void OnStreamOnline(object? sender, OnStreamOnlineArgs e)
    {
        var creator = _creators.Values.FirstOrDefault(c =>
            string.Equals(c.Slug, e.Stream.UserLogin, StringComparison.OrdinalIgnoreCase));
        if (creator is null) return;

        OnStreamUpdate.Invoke(this, new StreamStatusUpdate
        {
            CreatorId = creator.Id,
            IsLive = true,
            StartedAt = e.Stream.StartedAt,
        });
    }

    private void OnStreamOffline(object? sender, OnStreamOfflineArgs e)
    {
        var creator = _creators.Values.FirstOrDefault(c =>
            string.Equals(c.Slug, e.Channel, StringComparison.OrdinalIgnoreCase));

        if (creator is null) return;

        OnStreamUpdate.Invoke(this, new StreamStatusUpdate
        {
            CreatorId = creator.Id,
            IsLive = false,
            EndedAt = DateTime.UtcNow,
        });
    }

    private static IEnumerable<List<T>> SplitList<T>(List<T> items, int nSize = 30)
    {
        for (int i = 0; i < items.Count; i += nSize)
        {
            yield return items.GetRange(i, Math.Min(nSize, items.Count - i));
        }
    }
}