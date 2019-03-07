using System;
using System.Windows.Input;
#pragma warning disable CA1720 // Identifier contains type name

namespace GitHub.Services
{
    /// <summary>
    /// Service to broadcast messages, warnings and errors. Listeners
    /// can receive them by registering with <see cref="INotificationDispatcher"/>
    /// </summary>
    public interface INotificationService
    {
        void ShowMessage(string message);
        void ShowMessage(string message, ICommand command, bool showToolTips = true, Guid guid = default(Guid));
        void ShowWarning(string message);
        void ShowError(string message);
        void HideNotification(Guid guid);
        bool IsNotificationVisible(Guid guid);
    }
}