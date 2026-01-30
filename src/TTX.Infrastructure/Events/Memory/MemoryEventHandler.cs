using TTX.App.Events;

namespace TTX.Infrastructure.Events.Memory;

public sealed class MemoryEventHandler : IEventDispatcher, IEventReceiver
{
    private static readonly Queue<BaseEvent> _events = new();

    public Task Dispatch<T>(T @event) where T : BaseEvent
    {
        _events.Enqueue(@event);

        return Task.CompletedTask;
    }

    public async Task OnEventReceived<T>(Func<T, CancellationToken, Task> onEvent, CancellationToken cancellationToken = default) where T : BaseEvent
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_events.TryDequeue(out BaseEvent? @event) && @event is T typed)
            {
                await onEvent(typed, cancellationToken);
            }
        }
    }
}
