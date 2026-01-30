using System.Text.Json.Serialization;
using TTX.Domain.Models;
using TTX.App.Dto.Creators;

namespace TTX.App.Events.Creators;

public record UpdateStreamStatusEvent : BaseEvent
{
    [JsonPropertyName("creator_id")] public required int CreatorId { get; init; }
    [JsonPropertyName("stream_status")] public required StreamStatusDto StreamStatus { get; init; }

    public static UpdateStreamStatusEvent Create(Creator creator)
    {
        return new UpdateStreamStatusEvent
        {
            CreatorId = creator.Id,
            StreamStatus = StreamStatusDto.Create(creator.StreamStatus)
        };
    }
}
