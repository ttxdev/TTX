using TTX.Models;

namespace TTX.Notifications.Creators;

public class UpdateCreatorValue : INotification
{
    public required Vote Vote { get; init; }
}