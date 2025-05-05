using System.Text.Json.Serialization;
using TTX.Dto.CreatorApplications;
using TTX.Models;

namespace TTX.Notifications.CreatorApplications
{
    public class UpdateCreatorApplication : INotification
    {
        [JsonPropertyName("application")] public required CreatorApplicationDto Application { get; init; }

        public static UpdateCreatorApplication Create(CreatorApplication app)
        {
            return new UpdateCreatorApplication { Application = CreatorApplicationDto.Create(app) };
        }
    }
}