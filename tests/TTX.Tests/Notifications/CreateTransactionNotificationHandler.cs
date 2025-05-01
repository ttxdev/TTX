using MediatR;
using TTX.Notifications.Transactions;

namespace TTX.Tests.Notifications;

public class CreateTransactionNotificationHandler : TestNotificationHandler, INotificationHandler<CreateTransaction>
{
    public Task Handle(CreateTransaction notification, CancellationToken cancellationToken)
    {
        Notifications.Add(notification);
        return Task.CompletedTask;
    }
}
