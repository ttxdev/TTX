using Microsoft.AspNetCore.SignalR;

namespace TTX.Api.Hubs;

public class PortfolioHub : Hub
{
    public Task SetPlayer(int playerId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, $"player-{playerId}");
    }
}
