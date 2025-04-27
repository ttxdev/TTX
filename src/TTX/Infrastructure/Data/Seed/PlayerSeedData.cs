using System.Text.Json.Serialization;

namespace TTX.Infrastructure.Data.Seed;

public readonly struct PlayerSeedData
{
    [JsonPropertyName("name")]
    public string Name { get; init; }
    [JsonPropertyName("slug")]
    public string Slug { get; init; }
    [JsonPropertyName("twitch_id")]
    public string TwitchId { get; init; }
    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; init; }
}
