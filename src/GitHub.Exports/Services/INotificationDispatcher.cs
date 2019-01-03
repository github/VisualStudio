using System;
using System.Windows.Input;
using System.Diagnostics.CodeAnalysis;

namespace GitHub.Services
{
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct Notification
    {
        public enum NotificationType
        {
            Message,
            MessageCommand,
            Warning,
            Error
        }
        public string Message { get; }
        public NotificationType Type { get; }
        public ICommand Command { get; }

        public Notification(string message, NotificationType type, ICommand command = null)
        {
            Message = message;
            Type = type;
            Command = command;
        }
    }

    /// <summary>
    /// Dispatches notifications sent to the <see cref="INotificationService"/>
    /// to registered listeners.
    /// </summary>
    public interface INotificationDispatcher : INotificationService
    {
        IObservable<Notification> Listen();
        void AddListener(INotificationService notificationHandler);
        void RemoveListener();
        void RemoveListener(INotificationService notificationHandler);
    }
}