using System;
using System.Collections.Generic;
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
        static NotificationDispatcher()
        {
            System.Diagnostics.Debugger.Break();
        }


        Subject<Notification> notifications;
        Stack<INotificationService> notificationHandlers;

        public NotificationDispatcher()
        {
            notifications = new Subject<Notification>();
            notificationHandlers = new Stack<INotificationService>();
        }

        public IObservable<Notification> Listen()
        {
            return notifications;
        }

        public void AddListener(INotificationService handler)
        {
            notificationHandlers.Push(handler);
        }

        public void RemoveListener()
        {
            notificationHandlers.Pop();
        }

        public void RemoveListener(INotificationService handler)
        {
            Stack<INotificationService> handlers = new Stack<INotificationService>();
            while(notificationHandlers.TryPeek() != handler)
                handlers.Push(notificationHandlers.Pop());
            if (notificationHandlers.Count > 0)
                notificationHandlers.Pop();
            while (handlers.Count > 0)
                notificationHandlers.Push(handlers.Pop());
        }

        public void ShowMessage(string message)
        {
            notifications.OnNext(new Notification(message, Notification.NotificationType.Message));
            var handler = notificationHandlers.TryPeek();
            handler?.ShowMessage(message);
        }

        public void ShowMessage(string message, ICommand command, bool showToolTips = true, Guid guid = default(Guid))
        {
            notifications.OnNext(new Notification(message, Notification.NotificationType.Message, command));
            var handler = notificationHandlers.TryPeek();
            handler?.ShowMessage(message, command, showToolTips, guid);
        }

        public void ShowWarning(string message)
        {
            notifications.OnNext(new Notification(message, Notification.NotificationType.Warning));
            var handler = notificationHandlers.TryPeek();
            handler?.ShowWarning(message);
        }

        public void ShowError(string message)
        {
            notifications.OnNext(new Notification(message, Notification.NotificationType.Error));
            var handler = notificationHandlers.TryPeek();
            handler?.ShowError(message);
        }

        public void HideNotification(Guid guid)
        {
            var handler = notificationHandlers.TryPeek();
            handler?.HideNotification(guid);
        }

        public bool IsNotificationVisible(Guid guid)
        {
            var handler = notificationHandlers.TryPeek();
            return handler?.IsNotificationVisible(guid) ?? false;
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