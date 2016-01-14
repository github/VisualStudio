using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;

namespace GitHub.Services
{
    [Export(typeof(INotificationDispatcher))]
    [Export(typeof(INotificationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class NotificationDispatcher : INotificationDispatcher, IDisposable
    {
        Subject<Notification> notifications;

        public NotificationDispatcher()
        {
            notifications = new Subject<Notification>();
        }

        public IObservable<Notification> Listen()
        {
            return notifications;
        }

        public void ShowMessage(string message)
        {
            notifications.OnNext(new Notification(message, Notification.NotificationType.Message));
        }

        public void ShowMessage(string message, ICommand command)
        {
            notifications.OnNext(new Notification(message, Notification.NotificationType.Message, command));
        }

        public void ShowWarning(string message)
        {
            notifications.OnNext(new Notification(message, Notification.NotificationType.Warning));
        }

        public void ShowError(string message)
        {
            notifications.OnNext(new Notification(message, Notification.NotificationType.Error));
        }

        public void ClearNotifications()
        {
        }

        bool disposed; // To detect redundant calls
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;
                disposed = true;
                notifications.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}