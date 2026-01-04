using TTX.App.Events;

namespace TTX.Infrastructure.Events.Kafka;

public class KafkaEventDispatcher : IEventDispatcher
{
    public Task Dispatch<T>(T data) where T : IEvent
    {
        throw new NotImplementedException();
    }
}
