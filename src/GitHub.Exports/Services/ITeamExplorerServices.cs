using System.Windows.Input;

namespace GitHub.Services
{
    public interface ITeamExplorerServices : INotificationService
    {
        void ShowConnectPage();
        void ShowPublishSection();
        void ClearNotifications();
    }
}