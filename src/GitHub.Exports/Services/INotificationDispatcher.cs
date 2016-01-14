using System;
using System.Windows.Input;

namespace GitHub.Services
{
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

    public interface INotificationDispatcher : INotificationService
    {
        IObservable<Notification> Listen();
    }
}