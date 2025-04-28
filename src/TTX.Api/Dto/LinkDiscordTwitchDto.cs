using System.Text.Json.Serialization;

namespace TTX.Api.Dto;

public readonly struct LinkDiscordTwitchDto
{
    [JsonPropertyName("access_token")] public required string Token { get; init; }
    [JsonPropertyName("twitch_id")] public required string TwitchId { get; init; }
}