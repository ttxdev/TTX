using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace TTX.Api.Notifications;

public class RedisNotificationHandler<TNotification, THub>(
    ILogger logger,
    IConnectionMultiplexer redis,
    IHubContext<THub> hub) : BackgroundService where TNotification : INotification where THub : Hub
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var subscriber = redis.GetSubscriber();
        await subscriber.SubscribeAsync(typeof(TNotification).Name, async void (_, rMsg) =>
        {
            try
            {
                if (!rMsg.HasValue)
                    return;

                var parsed = JsonSerializer.Deserialize<TNotification>(rMsg.ToString());
                await hub.Clients.All.SendAsync(typeof(TNotification).Name, parsed, ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error processing Redis notification");
            }
        });
    }
}
