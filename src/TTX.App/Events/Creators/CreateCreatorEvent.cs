using System.Text.Json.Serialization;
using TTX.Domain.Models;
using TTX.App.Dto.Creators;

namespace TTX.App.Events.Creators;

public record CreateCreatorEvent : BaseEvent
{
    [JsonPropertyName("creator")] public required CreatorPartialDto Creator { get; init; }

    public static CreateCreatorEvent Create(Creator creator)
    {
        return new CreateCreatorEvent { Creator = CreatorPartialDto.Create(creator) };
    }
}
