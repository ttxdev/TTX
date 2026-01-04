using System.Text.Json.Serialization;
using TTX.Domain.Models;
using TTX.App.Dto.CreatorApplications;

namespace TTX.App.Events.CreatorApplications;

public class UpdateCreatorApplicationEvent : IEvent
{
    [JsonPropertyName("application")] public required CreatorApplicationDto Application { get; init; }

    public static UpdateCreatorApplicationEvent Create(CreatorApplication app)
    {
        return new UpdateCreatorApplicationEvent { Application = CreatorApplicationDto.Create(app) };
    }
}
