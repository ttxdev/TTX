using System.Text.Json.Serialization;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;

namespace TTX.App.Services.Creators;

public readonly struct OnboardRequest
{
    [JsonPropertyName("username")] public Slug? Username { get; init; }
    [JsonPropertyName("platform_id")] public PlatformId? PlatformId { get; init; }
    [JsonPropertyName("platform")] public required Platform Platform { get; init; }
    [JsonPropertyName("ticker")] public required Ticker Ticker { get; init; }
}
