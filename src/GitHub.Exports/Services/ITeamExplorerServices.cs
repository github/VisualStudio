using System.Windows.Input;

namespace GitHub.Services
{
    public interface ITeamExplorerServices : INotificationService
    {
        void ShowPublishSection();
        void ClearNotifications();
    }
}