using System.Text.Json.Serialization;
using TTX.Interfaces.Twitch;

namespace TTX.Api.Dto;

public class TwitchUserDto
{
    [JsonPropertyName("id")] public required string Id { get; init; }

    [JsonPropertyName("display_name")] public required string DisplayName { get; init; }

    [JsonPropertyName("login")] public required string Login { get; init; }

    [JsonPropertyName("avatar_url")] public required string AvatarUrl { get; init; }

    public static TwitchUserDto Create(TwitchUser user)
    {
        return new TwitchUserDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Login = user.Login,
            AvatarUrl = user.AvatarUrl.ToString()
        };
    }
}
