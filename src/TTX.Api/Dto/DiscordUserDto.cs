using System.Text.Json.Serialization;

namespace TTX.Api.Dto;

public readonly struct DiscordUserDto
{
    [JsonPropertyName("twitch_users")] public required TwitchUserDto[] TwitchUsers { get; init; }
}