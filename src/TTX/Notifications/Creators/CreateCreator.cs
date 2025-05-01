using TTX.Dto.Creators;
using TTX.Models;

namespace TTX.Notifications.Creators
{
    public class CreateCreator(Creator creator) : CreatorDto(creator), INotification;
}