namespace TTX.App.Events;

public interface IEventReceiver
{
    Task OnEventReceived<T>(Action<T, CancellationToken> onEvent, CancellationToken cancellationToken = default) where T : IEvent;
}
