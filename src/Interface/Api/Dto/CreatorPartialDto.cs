using System.Text.Json.Serialization;
using TTX.Core.Models;

namespace TTX.Interface.Api.Dto;

public class CreatorPartialDto : BaseDto<Creator>
{
    [JsonPropertyName("name")]
    public string Name { get; }
    [JsonPropertyName("slug")]
    public string Slug { get; }
    [JsonPropertyName("ticker")]
    public string Ticker { get; }
    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; }
    [JsonPropertyName("url")]
    public string Url => $"https://twitch.tv/{Slug}";
    [JsonPropertyName("value")]
    public long Value { get; }
    [JsonPropertyName("stream_status")]
    public StreamStatusDto StreamStatus { get; }

    public CreatorPartialDto(Creator creator) : base(creator)
    {
        Name = creator.Name;
        Slug = creator.Slug;
        Ticker = creator.Ticker;
        AvatarUrl = creator.AvatarUrl;
        Value = creator.Value;
        StreamStatus = new StreamStatusDto(creator.StreamStatus);
    }
}