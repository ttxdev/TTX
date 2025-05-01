using System.Text.Json;
using StackExchange.Redis;
using TTX.Interfaces.Events;
using TTX.Notifications;

namespace TTX.Infrastructure.Events
{
    public class RedisNotificationPublisher<T>(IConnectionMultiplexer redis)
        : INotificationPublisher<T> where T : INotification
    {
        [Obsolete]
        public async Task Handle(T notification, CancellationToken cancellationToken)
        {
            ISubscriber db = redis.GetSubscriber();

            string json = JsonSerializer.Serialize(notification);
            await db.PublishAsync(typeof(T).Name, json);
        }
    }
}