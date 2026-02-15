using System.Text.Json;
using StackExchange.Redis;
using TTX.App.Interfaces.Data.CreatorValue;
using TTX.Domain.ValueObjects;

namespace TTX.Infrastructure.Data.Repositories;

public class CreatorStatsRepository(IConnectionMultiplexer redis) : ICreatorStatsRepository
{
    public const string Key = "creator_stats";

    private static readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<CreatorStats> GetByCreator(Slug slug)
    {
        await _lock.WaitAsync();
        try
        {
            RedisValue creatorStatsData = await redis.GetDatabase().HashGetAsync(Key, slug.ToString());
            if (creatorStatsData.IsNullOrEmpty)
            {
                return new CreatorStats
                {
                    CreatorSlug = slug
                };
            }

            return JsonSerializer.Deserialize<CreatorStats>(creatorStatsData.ToString())!;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SetByCreator(Slug slug, CreatorStats stats)
    {
        await _lock.WaitAsync();
        try
        {
            await redis.GetDatabase().HashSetAsync(Key, slug.ToString(), JsonSerializer.Serialize(stats));
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<CreatorStats[]> GetAll(bool clear = true)
    {
        await _lock.WaitAsync();
        try
        {
            HashEntry[] creatorStatsData = await redis.GetDatabase().HashGetAllAsync(Key);
            List<CreatorStats> creatorStats = [];
            foreach (var entry in creatorStatsData)
            {
                creatorStats.Add(JsonSerializer.Deserialize<CreatorStats>(entry.Value.ToString())!);
            }

            if (clear)
            {
                await redis.GetDatabase().KeyDeleteAsync(Key);
            }

            return [.. creatorStats];
        }
        finally
        {
            _lock.Release();
        }
    }
}
