using System.Text.Json.Serialization;
using TTX.Dto.CreatorApplications;
using TTX.Models;

namespace TTX.Notifications.CreatorApplications
{
    public class CreateCreatorApplication : INotification
    {
        [JsonPropertyName("application")] public required CreatorApplicationDto Application { get; init; }

        public static CreateCreatorApplication Create(CreatorApplication app)
        {
            return new CreateCreatorApplication { Application = CreatorApplicationDto.Create(app) };
        }
    }
}