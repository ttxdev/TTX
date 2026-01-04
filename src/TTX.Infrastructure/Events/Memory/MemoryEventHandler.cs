using TTX.App.Events;

namespace TTX.Infrastructure.Events.Memory;

public sealed class MemoryEventHandler : IEventDispatcher, IEventReceiver
{
    private static readonly Queue<IEvent> _events = new();

    public Task Dispatch<T>(T @event) where T : IEvent
    {
        _events.Enqueue(@event);

        return Task.CompletedTask;
    }

    public async Task OnEventReceived<T>(Action<T, CancellationToken> onEvent, CancellationToken cancellationToken = default) where T : IEvent
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_events.TryDequeue(out IEvent? @event) && @event is T typedEvent)
            {
                onEvent(typedEvent, cancellationToken);
            }
        }
    }
}
