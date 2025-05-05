using System.Text.Json.Serialization;
using TTX.Dto.Creators;
using TTX.Models;

namespace TTX.Notifications.Creators
{
    public class UpdateStreamStatus : INotification
    {
        [JsonPropertyName("creator_id")] public required int CreatorId { get; init; }
        [JsonPropertyName("stream_status")] public required StreamStatusDto StreamStatus { get; init; }

        public static UpdateStreamStatus Create(Creator creator)
        {
            return new UpdateStreamStatus
            {
                CreatorId = creator.Id, StreamStatus = StreamStatusDto.Create(creator.StreamStatus)
            };
        }
    }
}