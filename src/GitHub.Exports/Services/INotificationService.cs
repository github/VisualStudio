using System.Windows.Input;

namespace GitHub.Services
{
    public interface INotificationService
    {
        void ShowMessage(string message);
        void ShowMessage(string message, ICommand command);
        void ShowWarning(string message);
        void ShowError(string message);
        void ClearNotifications();
    }
}