using MediatR;
using TTX.Notifications.Players;

namespace TTX.Tests.Notifications;

public class UpdatePlayerPortfolioNotificationHandler : TestNotificationHandler, INotificationHandler<UpdatePlayerPortfolio>
{
    public Task Handle(UpdatePlayerPortfolio notification, CancellationToken cancellationToken)
    {
        Notifications.Add(notification);
        return Task.CompletedTask;
    }
}