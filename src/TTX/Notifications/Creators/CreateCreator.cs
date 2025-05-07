using System.Text.Json.Serialization;
using TTX.Dto.Creators;
using TTX.Models;

namespace TTX.Notifications.Creators
{
    public class CreateCreator : INotification
    {
        [JsonPropertyName("creator")] public required CreatorPartialDto Creator { get; init; }

        public static CreateCreator Create(Creator creator)
        {
            return new CreateCreator { Creator = CreatorPartialDto.Create(creator) };
        }
    }
}