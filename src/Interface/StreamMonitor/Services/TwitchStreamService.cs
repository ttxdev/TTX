using Microsoft.Extensions.Logging;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TTX.Core.Models;
using TTX.Core.Interfaces;
using TTX.Interface.StreamMonitor.Provider;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace TTX.Interface.StreamMonitor.Services;

public class TwitchStreamMonitor : IStreamService
{
    private readonly TwitchAPI _twitchApi;
    private readonly LiveStreamMonitorService _liveStreamMonitor;
    private readonly Dictionary<string, Creator> _creators = [];
    private readonly ILogger<TwitchStreamMonitor> logger;
    public event EventHandler<StreamStatusUpdate> OnStreamUpdate = delegate { };

    public TwitchStreamMonitor(IConfigProvider config, ILogger<TwitchStreamMonitor> logger)
    {
        this.logger = logger;
        _twitchApi = new TwitchAPI();
        _twitchApi.Settings.ClientId = config.GetTwitchClientId();
        _twitchApi.Settings.Secret = config.GetTwitchClientSecret();

        _liveStreamMonitor = new LiveStreamMonitorService(_twitchApi);
        _liveStreamMonitor.OnStreamOnline += OnStreamOnline;
        _liveStreamMonitor.OnStreamOffline += OnStreamOffline;
    }

    public void AddCreator(Creator creator) => _creators[creator.Slug.ToLower()] = creator;
    public void RemoveCreator(Creator creator) => _creators.Remove(creator.Slug.ToLower());

    public async Task Start(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Twitch stream monitor...");
        logger.LogInformation($"Listening to {_creators.Count} creators");
        _liveStreamMonitor.SetChannelsByName(_creators.Values.Select(c => c.Slug.ToLower()).ToList());

        await OnReady();
        _liveStreamMonitor.Start();
        while (!cancellationToken.IsCancellationRequested);
    }

    private async Task OnReady()
    {
        logger.LogInformation("Initializing stream statuses for all creators...");

        try
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

            logger.LogInformation("Successfully initialized stream statuses");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initializing stream statuses");
        }
    }

    private void OnStreamOnline(object? sender, OnStreamOnlineArgs e)
    {
        logger.LogInformation("Stream online: {UserName}", e.Stream.UserName);

        var creator = _creators.Values.FirstOrDefault(c =>
            string.Equals(c.Slug, e.Stream.UserLogin, StringComparison.OrdinalIgnoreCase));
        if (creator is null)
        {
            logger.LogWarning("Creator not found for channel: {Channel}", e.Channel);
            return;
        }

        OnStreamUpdate.Invoke(this, new StreamStatusUpdate
        {
            CreatorId = creator.Id,
            IsLive = true,
            StartedAt = e.Stream.StartedAt,
        });
    }

    private void OnStreamOffline(object? sender, OnStreamOfflineArgs e)
    {
        logger.LogInformation("Stream offline: {Channel}", e.Channel);

        var creator = _creators.Values.FirstOrDefault(c =>
            string.Equals(c.Slug, e.Channel, StringComparison.OrdinalIgnoreCase));

        if (creator is null)
        {
            logger.LogWarning("Creator not found for channel: {Channel}", e.Channel);
            return;
        }

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