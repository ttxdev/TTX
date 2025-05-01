using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Dto.Creators
{
    public class VoteDto(Vote vote)
    {
        [JsonPropertyName("creator_id")] public int CreatorId { get; } = vote.CreatorId;

        [JsonPropertyName("value")] public long Value { get; } = vote.Value;

        [JsonPropertyName("time")] public DateTimeOffset Time { get; } = vote.Time;
    }
}