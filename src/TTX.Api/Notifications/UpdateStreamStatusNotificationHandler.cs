using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using TTX.Api.Hubs;
using TTX.Notifications.Creators;

namespace TTX.Api.Notifications;

public class UpdateStreamStatusNotificationHandler(ILogger<UpdateStreamStatusNotificationHandler> logger, IConnectionMultiplexer redis, IHubContext<EventHub> hub) : RedisNotificationHandler<UpdateStreamStatus, EventHub>(logger, redis, hub);