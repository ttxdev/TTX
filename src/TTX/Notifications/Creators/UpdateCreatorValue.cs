using TTX.Dto.Creators;
using TTX.Models;

namespace TTX.Notifications.Creators
{
    public class UpdateCreatorValue(Vote vote) : VoteDto(vote), INotification;
}