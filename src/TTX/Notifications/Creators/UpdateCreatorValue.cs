using System.Text.Json.Serialization;
using TTX.Dto.Creators;
using TTX.Models;

namespace TTX.Notifications.Creators
{
    public class UpdateCreatorValue : INotification
    {
        [JsonPropertyName("vote")] public required VoteDto Vote { get; init; }

        public static UpdateCreatorValue Create(Vote vote)
        {
            return new UpdateCreatorValue { Vote = VoteDto.Create(vote) };
        }
    }
}