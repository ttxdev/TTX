using MediatR;
using TTX.Notifications.Creators;

namespace TTX.Tests.Notifications;

public class CreateCreatorNotificationHandler : TestNotificationHandler, INotificationHandler<CreateCreator>
{
    public Task Handle(CreateCreator notification, CancellationToken cancellationToken)
    {
        Notifications.Add(notification);
        return Task.CompletedTask;
    }
}