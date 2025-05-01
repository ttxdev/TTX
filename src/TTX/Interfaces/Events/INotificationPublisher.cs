using MediatR;
using INotification = TTX.Notifications.INotification;

namespace TTX.Interfaces.Events
{
    public interface INotificationPublisher<in T> : INotificationHandler<T> where T : INotification;
}