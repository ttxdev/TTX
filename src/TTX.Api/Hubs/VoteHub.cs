using Microsoft.AspNetCore.SignalR;
using TTX.Domain.ValueObjects;

namespace TTX.Api.Hubs;

public class VoteHub : Hub
{
    public Task SetCreator(int creatorId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, $"creator-{creatorId}");
    }
}
