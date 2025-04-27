using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Api.Dto;

public class UserDto(User user) : BaseDto<User>(user)
{
    [JsonPropertyName("name")]
    [JsonPropertyOrder(10)]
    public string Name { get; } = user.Name;
    [JsonPropertyName("slug")]
    [JsonPropertyOrder(12)]
    public string Slug { get; } = user.Slug;
    [JsonPropertyName("url")]
    [JsonPropertyOrder(14)]
    public string Url => $"https://twitch.tv/{Slug}";
    [JsonPropertyName("avatar_url")]
    [JsonPropertyOrder(16)]
    public string AvatarUrl { get; } = user.AvatarUrl.ToString();
}
