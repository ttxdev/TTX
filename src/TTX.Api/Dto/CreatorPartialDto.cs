using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Api.Dto;

public class CreatorPartialDto(Creator creator) : UserDto(creator)
{
    [JsonPropertyName("ticker")]
    [JsonPropertyOrder(11)]
    public string Ticker { get; } = creator.Ticker;
    [JsonPropertyName("value")]
    [JsonPropertyOrder(13)]
    public long Value { get; } = creator.Value;
    [JsonPropertyName("stream_status")]
    [JsonPropertyOrder(15)]
    public StreamStatusDto StreamStatus { get; } = new StreamStatusDto(creator.StreamStatus);
    [JsonPropertyName("history")]
    [JsonPropertyOrder(19)]
    public VoteDto[] History { get; } = [.. creator.History.Select(v => new VoteDto(v))];
}