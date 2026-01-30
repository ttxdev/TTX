using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TTX.App.Events;

namespace TTX.Infrastructure.Events.Redis;

public class RedisEventReceiver(ILogger<RedisEventReceiver> _logger, IConnectionMultiplexer _redis) : IEventReceiver
{
    private static RedisChannel Channel => RedisEventDispatcher.Channel;

    public async Task OnEventReceived<T>(Func<T, CancellationToken, Task> onEvent, CancellationToken cancellationToken = default) where T : BaseEvent
    {
        async void handler(RedisChannel _, RedisValue message)
        {
            try
            {
                if (message.IsNullOrEmpty) return;

                using JsonDocument doc = JsonDocument.Parse((byte[])message!);

                if (doc.RootElement.TryGetProperty("type", out var typeProp) &&
                    typeProp.ValueEquals(typeof(T).Name))
                {
                    T? typed = doc.Deserialize<T>();

                    if (typed is not null)
                    {
                        await onEvent(typed, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing event");
            }
        }

        ISubscriber db = _redis.GetSubscriber();
        await db.SubscribeAsync(Channel, handler);

        TaskCompletionSource<object?> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using (cancellationToken.Register(() => tcs.TrySetResult(null)))
        {
            await tcs.Task.ConfigureAwait(false);
        }

        await db.UnsubscribeAsync(Channel, handler).ConfigureAwait(false);
    }
}
