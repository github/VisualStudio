using System.Windows.Input;

namespace GitHub.Services
{
    /// <summary>
    /// Service to broadcast messages, warnings and errors. Listeners
    /// can receive them by registering with <see cref="INotificationDispatcher"/>
    /// </summary>
    public interface INotificationService
    {
        void ShowMessage(string message);
        void ShowMessage(string message, ICommand command);
        void ShowWarning(string message);
        void ShowError(string message);
    }
}