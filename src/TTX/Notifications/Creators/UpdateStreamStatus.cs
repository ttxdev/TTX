using System.Text.Json.Serialization;
using TTX.Dto.Creators;
using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Notifications.Creators
{
    public class UpdateStreamStatus(Creator creator) : StreamStatusDto(creator.StreamStatus), INotification
    {
        [JsonPropertyName("creator_id")] public ModelId CreatorId { get; } = creator.Id;
    }
}