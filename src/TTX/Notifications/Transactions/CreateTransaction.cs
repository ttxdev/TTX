using TTX.Models;

namespace TTX.Notifications.Transactions;

public class CreateTransaction : INotification
{
    public required Transaction Transaction { get; init; }
}
