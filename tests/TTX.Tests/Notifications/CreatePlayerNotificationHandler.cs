using MediatR;
using TTX.Notifications.Players;

namespace TTX.Tests.Notifications;

public class CreatePlayerNotificationHandler : TestNotificationHandler, INotificationHandler<CreatePlayer>
{
    public Task Handle(CreatePlayer notification, CancellationToken cancellationToken)
    {
        Notifications.Add(notification);
        return Task.CompletedTask;
    }
}