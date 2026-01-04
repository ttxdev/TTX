using TTX.App.Events;

namespace TTX.Infrastructure.Events.Kafka;

public class KafkaEventReceiver : IEventReceiver
{
    public Task OnEventReceived<T>(Action<T, CancellationToken> onEvent, CancellationToken cancellationToken = default) where T : IEvent
    {
        throw new NotImplementedException();
    }
}
