using System.Text.Json.Serialization;

namespace TTX.Interface.Api.Dto;

public class DiscordDto
{
    [JsonPropertyName("access_token")]
    public required string Token { get; set; }
    [JsonPropertyName("users")]
    public required DiscordTwitchDto[] Users { get; set; }
}

public class DiscordTwitchDto
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("display_name")]
    public required string DisplayName { get; set; }
    [JsonPropertyName("login")]
    public required string Login { get; set; }
    [JsonPropertyName("avatar_url")]
    public required string AvatarUrl { get; set; }

}