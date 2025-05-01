using System.Text.Json.Serialization;
using TTX.ValueObjects;

namespace TTX.Dto
{
    public class UserDto : BaseDto
    {
        [JsonPropertyName("name")] public required string Name { get; init; }

        [JsonPropertyName("slug")] public required string Slug { get; init; }

        [JsonPropertyName("twitch_id")] public required TwitchId TwitchId { get; init; }

        [JsonPropertyName("url")] public string Url => $"https://twitch.tv/{Slug}";

        [JsonPropertyName("avatar_url")] public required string AvatarUrl { get; init; }
    }
}