using System.Text.Json.Serialization;
using TTX.Domain.Models;

namespace TTX.App.Dto.Creators;

public class StreamStatusDto
{
    [JsonPropertyName("is_live")] public required bool IsLive { get; init; }
    [JsonPropertyName("started_at")] public DateTimeOffset? StartedAt { get; init; }
    [JsonPropertyName("ended_at")] public DateTimeOffset? EndedAt { get; init; }

    public static StreamStatusDto Create(StreamStatus streamStatus)
    {
        return new StreamStatusDto
        {
            IsLive = streamStatus.IsLive,
            StartedAt = streamStatus.StartedAt,
            EndedAt = streamStatus.EndedAt
        };
    }
}
