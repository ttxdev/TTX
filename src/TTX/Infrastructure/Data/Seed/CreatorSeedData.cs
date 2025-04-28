using System.Text.Json.Serialization;

namespace TTX.Infrastructure.Data.Seed
{
    public readonly struct PCreatorSeedData
    {
        [JsonPropertyName("id")] public string Id { get; init; }
        [JsonPropertyName("login")] public string Login { get; init; }
        [JsonPropertyName("display_name")] public string DisplayName { get; init; }
        [JsonPropertyName("ticker")] public string Ticker { get; init; }
        [JsonPropertyName("type")] public string Type { get; init; }
        [JsonPropertyName("broadcaster_type")] public string BroadcasterType { get; init; }
        [JsonPropertyName("description")] public string Description { get; init; }

        [JsonPropertyName("profile_image_url")]
        public string ProfileImageUrl { get; init; }

        [JsonPropertyName("offline_image_url")]
        public string OfflineImageUrl { get; init; }

        [JsonPropertyName("view_count")] public int ViewCount { get; init; }
        [JsonPropertyName("created_at")] public DateTime CreatedAt { get; init; }
    }
}