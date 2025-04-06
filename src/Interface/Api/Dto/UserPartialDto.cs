using System.Text.Json.Serialization;
using TTX.Core.Models;

namespace TTX.Interface.Api.Dto;

public class UserPartialDto : BaseDto<User>
{
  [JsonPropertyName("name")]
  public string Name { get; }
  [JsonPropertyName("slug")]
  public string Slug { get; }
  [JsonPropertyName("avatar_url")]
  public string AvatarUrl { get; }
  [JsonPropertyName("credits")]
  public long Credits { get; }
  [JsonPropertyName("url")]
  public string Url => $"https://twitch.tv/{Name}";
  [JsonPropertyName("type")]
  public UserType Type { get; }

  public UserPartialDto(User user) : base(user)
  {
    Name = user.Name;
    Slug = user.Name;
    AvatarUrl = user.AvatarUrl;
    Credits = user.Credits;
    Type = user.Type;
  }
}