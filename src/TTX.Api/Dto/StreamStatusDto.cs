using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Api.Dto;

public class StreamStatusDto(StreamStatus streamStatus)
{
    [JsonPropertyName("is_live")] public bool IsLive { get; } = streamStatus.IsLive;

    [JsonPropertyName("started_at")] public DateTimeOffset? StartedAt { get; } = streamStatus.StartedAt;

    [JsonPropertyName("ended_at")] public DateTimeOffset? EndedAt { get; } = streamStatus.EndedAt;
}