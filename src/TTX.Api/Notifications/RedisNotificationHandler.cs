using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace TTX.Api.Notifications;

public class RedisNotificationHandler<TNotification, THub>(ILogger logger, IConnectionMultiplexer redis, IHubContext<THub> hub) : BackgroundService where TNotification : INotification where THub : Hub
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        ISubscriber subscriber = redis.GetSubscriber();
        await subscriber.SubscribeAsync(typeof(TNotification).Name, async void (_, rMsg) =>
        {
            try
            {
                if (!rMsg.HasValue)
                    return;
            
                using var doc = JsonDocument.Parse(rMsg.ToString());
                var jsonPayload = doc.RootElement;
                await hub.Clients.All.SendAsync(typeof(TNotification).Name, jsonPayload, ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error processing Redis notification");
            }
        });
    }
}