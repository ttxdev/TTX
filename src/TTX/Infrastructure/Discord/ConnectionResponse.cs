using System.Text.Json.Serialization;

namespace TTX.Infrastructure.Discord
{
    public readonly struct ConnectionResponse
    {
        [JsonPropertyName("id")] public required string Id { get; init; }
        [JsonPropertyName("type")] public required string Type { get; init; }
        [JsonPropertyName("verified")] public required bool Verified { get; init; }
    }
}