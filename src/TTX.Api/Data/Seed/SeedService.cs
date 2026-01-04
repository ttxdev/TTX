using System.Text.Json;
using System.Text.Json.Serialization;
using TTX.Domain.Models;
using TTX.Infrastructure.Data;

namespace TTX.Api.Data.Seed;

public class SeedService()
{
    public static async Task Seed(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
    {
        CreatorSeedData[] creatorData = await GetCreators(cancellationToken);
        PlayerSeedData[] playerData = await GetPlayers(cancellationToken);

        foreach (PlayerSeedData data in playerData)
        {
            dbContext.Players.Add(new Player
            {
                Name = data.Name,
                Slug = data.Slug,
                PlatformId = data.PlatformId,
                Platform = data.Platform,
                AvatarUrl = new Uri(data.AvatarUrl),
                Type = data.Type,
            });
        }

        foreach (CreatorSeedData data in creatorData)
        {
            dbContext.Creators.Add(new Creator
            {
                Name = data.Name,
                Slug = data.Slug,
                PlatformId = data.PlatformId,
                Platform = data.Platform,
                AvatarUrl = new Uri(data.AvatarUrl),
                Ticker = data.Ticker,
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<CreatorSeedData[]> GetCreators(CancellationToken cancellationToken = default)
    {
        string data = await File.ReadAllTextAsync("Data/Seed/creators.json", cancellationToken);
        return JsonSerializer.Deserialize<CreatorSeedData[]>(data)!;
    }

    private static async Task<PlayerSeedData[]> GetPlayers(CancellationToken cancellationToken = default)
    {
        string data = await File.ReadAllTextAsync("Data/Seed/players.json", cancellationToken);
        return JsonSerializer.Deserialize<PlayerSeedData[]>(data)!;
    }
}

internal record UserSeedData
{
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("slug")] public required string Slug { get; init; }
    [JsonPropertyName("platform")] public required Platform Platform { get; init; }
    [JsonPropertyName("platform_id")] public required string PlatformId { get; init; }
    [JsonPropertyName("avatar_url")] public required string AvatarUrl { get; init; }
}

internal record PlayerSeedData : UserSeedData
{
    [JsonPropertyName("type")] public required PlayerType Type { get; init; }
}

internal record CreatorSeedData : UserSeedData
{
    [JsonPropertyName("ticker")] public required string Ticker { get; init; }
}
