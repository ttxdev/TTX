using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using TTX.Api.Hubs;
using TTX.Notifications.Players;

namespace TTX.Api.Notifications;

public class UpdatePlayerPortfolioNotificationHandler(
    ILogger<UpdatePlayerPortfolioNotificationHandler> logger,
    IConnectionMultiplexer redis,
    IHubContext<EventHub> hub) : RedisNotificationHandler<UpdatePlayerPortfolio, EventHub>(logger, redis, hub);