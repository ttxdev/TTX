using System.Text.Json.Serialization;
using TTX.Core.Models;

namespace TTX.Interface.Api.Dto;

public class StreamStatusDto(StreamStatus streamStatus)
{
    [JsonPropertyName("is_live")]
    public bool IsLive { get; } = streamStatus.IsLive;
    [JsonPropertyName("started_at")]
    public DateTime? StartedAt { get; } = streamStatus.StartedAt;
    [JsonPropertyName("ended_at")]
    public DateTime? EndedAt { get; } = streamStatus.EndedAt;
}