using TTX.Models;

namespace TTX.Notifications.Players;

public class CreatePlayer : INotification
{
    public required Player Player { get; init; }
}
