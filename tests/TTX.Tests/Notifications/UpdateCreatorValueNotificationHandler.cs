using MediatR;
using TTX.Notifications.Creators;

namespace TTX.Tests.Notifications;

public class UpdateCreatorValueNotificationHandler : TestNotificationHandler, INotificationHandler<UpdateCreatorValue>
{
    public Task Handle(UpdateCreatorValue notification, CancellationToken cancellationToken)
    {
        Notifications.Add(notification);
        return Task.CompletedTask;
    }
}