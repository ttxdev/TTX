using MediatR;

namespace TTX.Tests.Notifications;

public class TestNotificationHandler
{
    public static HashSet<INotification> Notifications { get; } = [];
    
    public T? FindNotification<T>(Func<T, bool> findCb) where T : INotification => Notifications.OfType<T>().FirstOrDefault(findCb);
}