using MediatR;
using TTX.Notifications.LootBoxes;

namespace TTX.Tests.Notifications;

public class OpenLootBoxNotificationHandler : TestNotificationHandler, INotificationHandler<OpenLootBox>
{
    public Task Handle(OpenLootBox notification, CancellationToken cancellationToken)
    {
        Notifications.Add(notification);
        return Task.CompletedTask;
    }
}