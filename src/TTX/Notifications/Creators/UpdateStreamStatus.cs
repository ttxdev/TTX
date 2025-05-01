using System.Text.Json.Serialization;
using TTX.Dto.Creators;
using TTX.Models;

namespace TTX.Notifications.Creators
{
    public class UpdateStreamStatus : StreamStatusDto, INotification
    {
        [JsonPropertyName("creator_id")] public required int CreatorId { get; init; }

        public static UpdateStreamStatus Create(Creator creator)
        {
            return new UpdateStreamStatus
            {
                CreatorId = creator.Id,
                IsLive = creator.StreamStatus.IsLive,
                StartedAt = creator.StreamStatus.StartedAt,
                EndedAt = creator.StreamStatus.EndedAt
            };
        }
    }
}