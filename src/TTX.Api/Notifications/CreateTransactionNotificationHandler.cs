using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using TTX.Api.Hubs;
using TTX.Notifications.Transactions;

namespace TTX.Api.Notifications;

public class CreateTransactionNotificationHandler(ILogger<CreateTransactionNotificationHandler> logger, IConnectionMultiplexer redis, IHubContext<EventHub> hub) : RedisNotificationHandler<CreateTransaction, EventHub>(logger, redis, hub);