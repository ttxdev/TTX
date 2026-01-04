using System.Text.Json.Serialization;
using TTX.Domain.Models;

namespace TTX.App.Dto.Creators;

public class VoteDto
{
    [JsonPropertyName("creator_id")] public required int CreatorId { get; init; }
    [JsonPropertyName("value")] public required long Value { get; init; }
    [JsonPropertyName("time")] public required DateTimeOffset Time { get; init; }

    public static VoteDto Create(Vote vote)
    {
        return new VoteDto { CreatorId = vote.CreatorId, Value = vote.Value, Time = vote.Time };
    }
}
