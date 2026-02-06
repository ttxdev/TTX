using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TTX.App.Jobs.Streams;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;
using TTX.Infrastructure.Options;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;

namespace TTX.Infrastructure.Twitch;

public class TwitchStreamMonitorAdapter : IStreamMonitorAdapter
{
    public event EventHandler<StreamUpdateEvent>? StreamStatusUpdated;
    private readonly ILogger<TwitchStreamMonitorAdapter> _logger;
    private readonly TwitchOAuthOptions _authOptions;
    private readonly TwitchStreamMonitorOptions _monitorOptions;
    private readonly TwitchAPI _twitchApi;
    private readonly ConcurrentDictionary<string, Creator> _watchedCreators = new();
    private readonly ConcurrentDictionary<ModelId, StreamState> _lastKnownStates = new();
    private readonly SemaphoreSlim _syncLock = new(1, 1);

    public TwitchStreamMonitorAdapter(
        IOptions<TwitchOAuthOptions> twitchOptions,
        IOptions<TwitchStreamMonitorOptions> streamMonitorOptions,
        ILogger<TwitchStreamMonitorAdapter> logger)
    {
        _logger = logger;
        _authOptions = twitchOptions.Value;
        _monitorOptions = streamMonitorOptions.Value;

        _twitchApi = new TwitchAPI
        {
            Settings =
            {
                ClientId = _authOptions.ClientId,
                Secret = _authOptions.ClientSecret
            }
        };
    }

    public async Task Start(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Twitch Stream Monitor...");

        while (!cancellationToken.IsCancellationRequested)
        {
            await CheckStreamsAsync(cancellationToken);
            await Task.Delay(_monitorOptions.Delay, cancellationToken);
        }
    }

    public void SetCreators(IEnumerable<Creator> creators)
    {
        _syncLock.Wait();
        try
        {
            _watchedCreators.Clear();
            foreach (Creator creator in creators)
            {
                _watchedCreators[creator.Slug.Value.ToLowerInvariant()] = creator;

                if (!_lastKnownStates.ContainsKey(creator.Id))
                {
                    _lastKnownStates[creator.Id] = new StreamState(false, null);
                }
            }
        }
        finally
        {
            _syncLock.Release();
        }
    }

    public bool RemoveCreator(ModelId creatorId)
    {
        Creator? target = _watchedCreators.Values.FirstOrDefault(c => c.Id == creatorId);
        if (target is null) return false;

        _lastKnownStates.TryRemove(creatorId, out _);
        return _watchedCreators.TryRemove(target.Slug.Value.ToLowerInvariant(), out _);
    }

    private async Task CheckStreamsAsync(CancellationToken ct)
    {
        List<string> slugsToCheck;

        await _syncLock.WaitAsync(ct);
        try
        {
            if (_watchedCreators.IsEmpty) return;
            slugsToCheck = [.. _watchedCreators.Keys];
        }
        finally
        {
            _syncLock.Release();
        }

        foreach (string[] chunk in slugsToCheck.Chunk(100))
        {
            if (ct.IsCancellationRequested) break;

            GetStreamsResponse? apiResponse = await _twitchApi.Helix.Streams.GetStreamsAsync(userLogins: [.. chunk]);

            if (apiResponse == null) continue;

            var liveStreamMap = apiResponse.Streams.ToDictionary(k => k.UserLogin.ToLowerInvariant(), v => v);

            foreach (string? slug in chunk)
            {
                if (!_watchedCreators.TryGetValue(slug, out Creator? creator))
                {
                    continue;
                }

                bool isLiveNow = liveStreamMap.TryGetValue(slug, out var streamData);

                await ProcessCreatorState(creator, isLiveNow, streamData?.StartedAt);
            }
        }
    }

    private async Task ProcessCreatorState(Creator creator, bool isLiveNow, DateTime? startedAt)
    {
        if (!_lastKnownStates.TryGetValue(creator.Id, out StreamState? lastState))
        {
            lastState = new StreamState(false, null);
        }

        if (isLiveNow && !lastState.IsLive)
        {
            FireEvent(creator.Id, true, startedAt ?? DateTime.UtcNow);
            _lastKnownStates[creator.Id] = new StreamState(true, startedAt);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Creator {Creator} went live.", creator.Slug);
            }
        }

        FireEvent(creator.Id, false, DateTime.UtcNow);
        _lastKnownStates[creator.Id] = new StreamState(false, null);
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Creator {Creator} went offline.", creator.Slug);
        }
    }

    private void FireEvent(ModelId creatorId, bool isLive, DateTime at)
    {
        StreamStatusUpdated?.Invoke(this, new StreamUpdateEvent
        {
            CreatorId = creatorId,
            IsLive = isLive,
            At = at
        });
    }

    private record StreamState(bool IsLive, DateTime? StartedAt);
}
