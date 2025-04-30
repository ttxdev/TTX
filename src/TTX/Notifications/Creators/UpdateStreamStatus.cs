using TTX.Models;

namespace TTX.Notifications.Creators;

public class UpdateStreamStatus : INotification
{
    public required StreamStatus StreamStatus { get; init; }
}
