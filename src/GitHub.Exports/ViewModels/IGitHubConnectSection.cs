using GitHub.Models;
using System.Windows.Input;

namespace GitHub.VisualStudio.TeamExplorer.Connect
{
    public interface IGitHubConnectSection
    {
        void DoCreate();
        void SignOut();
        void Login();
        void Retry();
        bool OpenRepository();
        string ErrorMessage { get; }
        IConnection SectionConnection { get; }
        bool IsLoggingIn { get; }
        bool ShowLogin { get; }
        bool ShowLogout { get; }
        bool ShowRetry { get; }
        ICommand Clone { get; }
    }
}
