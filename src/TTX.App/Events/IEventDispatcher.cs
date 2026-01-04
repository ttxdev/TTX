namespace TTX.App.Events;

public interface IEventDispatcher
{
    public Task Dispatch<T>(T @event) where T : IEvent;
}
