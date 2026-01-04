using System.Text.Json;
using StackExchange.Redis;
using TTX.App.Events;

namespace TTX.Infrastructure.Events.Redis;

public class RedisEventReceiver(IConnectionMultiplexer _redis) : IEventReceiver
{
    private static RedisChannel Channel => RedisEventDispatcher.Channel;

    public async Task OnEventReceived<T>(Action<T, CancellationToken> onEvent, CancellationToken cancellationToken = default) where T : IEvent
    {
        async void handler(RedisChannel _, RedisValue message)
        {
            if (!message.HasValue)
            {
                return;
            }

            IEvent? @event = JsonSerializer.Deserialize<IEvent>(message.ToString());
            if (@event is T typed)
            {
                onEvent.Invoke(typed, cancellationToken);
            }
        }

        ISubscriber db = _redis.GetSubscriber();
        await db.SubscribeAsync(Channel, handler);
        while (!cancellationToken.IsCancellationRequested)
        { }

        await db.UnsubscribeAsync(Channel, handler);
    }
}
