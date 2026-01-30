using System.Text.Json.Serialization;
using TTX.App.Dto.CreatorApplications;
using TTX.App.Events;
using TTX.Domain.Models;

namespace TTX.App.Events.CreatorApplications;

public record CreateCreatorApplicationEvent : BaseEvent
{
    [JsonPropertyName("application")] public required CreatorApplicationDto Application { get; init; }

    public static CreateCreatorApplicationEvent Create(CreatorApplication app)
    {
        return new CreateCreatorApplicationEvent { Application = CreatorApplicationDto.Create(app) };
    }
}
