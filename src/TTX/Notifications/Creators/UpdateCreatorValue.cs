using TTX.Dto.Creators;
using TTX.Models;

namespace TTX.Notifications.Creators
{
    public class UpdateCreatorValue : VoteDto, INotification
    {
        public static new UpdateCreatorValue Create(Vote vote)
        {
            return new UpdateCreatorValue { CreatorId = vote.CreatorId, Value = vote.Value, Time = vote.Time };
        }
    }
}