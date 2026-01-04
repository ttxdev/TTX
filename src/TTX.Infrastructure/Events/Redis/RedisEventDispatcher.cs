using System.Text.Json;
using StackExchange.Redis;
using TTX.App.Events;

namespace TTX.Infrastructure.Events.Redis;

public class RedisEventDispatcher(IConnectionMultiplexer _redis) : IEventDispatcher
{
    public static readonly RedisChannel Channel = RedisChannel.Literal("TTX.Events");

    public async Task Dispatch<T>(T @event) where T : IEvent
    {
        ISubscriber db = _redis.GetSubscriber();
        await db.PublishAsync(Channel, JsonSerializer.Serialize(@event));
    }
}
