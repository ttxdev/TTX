namespace TTX.App.Events;

public interface IEventReceiver
{
    Task OnEventReceived<T>(Func<T, CancellationToken, Task> onEvent, CancellationToken cancellationToken = default) where T : BaseEvent;
}
