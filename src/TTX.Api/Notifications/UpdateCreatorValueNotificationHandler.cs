using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using TTX.Api.Hubs;
using TTX.Notifications.Creators;

namespace TTX.Api.Notifications;

public class UpdateCreatorValueNotificationHandler(
    ILogger<UpdateCreatorValueNotificationHandler> logger,
    IConnectionMultiplexer redis,
    IHubContext<EventHub> hub) : RedisNotificationHandler<UpdateCreatorValue, EventHub>(logger, redis, hub);