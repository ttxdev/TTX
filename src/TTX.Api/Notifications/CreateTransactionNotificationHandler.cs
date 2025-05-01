using MediatR;
using Microsoft.AspNetCore.SignalR;
using TTX.Api.Hubs;
using TTX.Notifications.Transactions;

namespace TTX.Api.Notifications;

public class CreateTransactionNotificationHandler(IHubContext<EventHub> hub) : INotificationHandler<CreateTransaction>
{
    public Task Handle(CreateTransaction notification, CancellationToken cancellationToken)
    {
        return hub.Clients.All.SendAsync("CreateTransaction", notification, cancellationToken: cancellationToken);
    }
}