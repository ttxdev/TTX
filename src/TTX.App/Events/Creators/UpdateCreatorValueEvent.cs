using System.Text.Json.Serialization;
using TTX.Domain.Models;
using TTX.App.Dto.Creators;

namespace TTX.App.Events.Creators;

public record UpdateCreatorValueEvent : IEvent
{
    [JsonPropertyName("vote")] public required VoteDto Vote { get; init; }

    public static UpdateCreatorValueEvent Create(Vote vote)
    {
        return new UpdateCreatorValueEvent { Vote = VoteDto.Create(vote) };
    }
}
