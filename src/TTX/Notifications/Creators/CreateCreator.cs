using TTX.Models;

namespace TTX.Notifications.Creators;

public class CreateCreator : INotification
{
    public required Creator Creator { get; init; }
}
