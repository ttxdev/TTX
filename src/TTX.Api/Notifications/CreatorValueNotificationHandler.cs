using MediatR;
using Microsoft.AspNetCore.SignalR;
using TTX.Api.Hubs;
using TTX.Notifications.Creators;

namespace TTX.Api.Notifications;

public class CreatorValueNotificationHandler(IHubContext<EventHub> hub) : INotificationHandler<UpdateCreatorValue>
{
    public Task Handle(UpdateCreatorValue notification, CancellationToken cancellationToken)
    {
        return hub.Clients.All.SendAsync("UpdateCreatorValue", notification, cancellationToken: cancellationToken);
    }
}