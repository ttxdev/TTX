using System.Text.Json.Serialization;
using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Dto
{
    public class UserDto(User user) : BaseDto(user)
    {
        [JsonPropertyName("name")]
        [JsonPropertyOrder(10)]
        public string Name { get; } = user.Name;

        [JsonPropertyName("slug")]
        [JsonPropertyOrder(12)]
        public string Slug { get; } = user.Slug;

        [JsonPropertyName("twitch_id")]
        [JsonPropertyOrder(13)]
        public TwitchId TwitchId { get; } = user.TwitchId;

        [JsonPropertyName("url")]
        [JsonPropertyOrder(14)]
        public string Url => $"https://twitch.tv/{Slug}";

        [JsonPropertyName("avatar_url")]
        [JsonPropertyOrder(16)]
        public string AvatarUrl { get; } = user.AvatarUrl.ToString();
    }
}