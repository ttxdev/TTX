using System.Text.Json.Serialization;
using TTX.Domain.Models;

namespace TTX.App.Dto;

public class UserDto : BaseDto
{
    [JsonPropertyName("name")] public required string Name { get; init; }

    [JsonPropertyName("slug")] public required string Slug { get; init; }

    [JsonPropertyName("platform_id")] public required string PlatformId { get; init; }

    [JsonPropertyName("platform")] public required Platform Platform { get; init; }

    [JsonPropertyName("platform_url")]
    public string PlatformUrl => Platform switch
    {
        Platform.Twitch => $"https://twitch.tv/{Slug}",
        _ => $"https://twitch.tv/{Slug}"
    };

    [JsonPropertyName("avatar_url")] public required string AvatarUrl { get; init; }
}
