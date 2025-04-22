using System.Text.Json.Serialization;
using TTX.Interfaces.Twitch;

namespace TTX.Api.Dto;

public class TwitchUserDto(TwitchUser user)
{
    [JsonPropertyName("id")]
    public string Id { get; } = user.Id;
    [JsonPropertyName("display_name")]
    public string DisplayName { get; } = user.DisplayName;
    [JsonPropertyName("login")]
    public string Login { get; } = user.Login;
    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; } = user.AvatarUrl.ToString();
}