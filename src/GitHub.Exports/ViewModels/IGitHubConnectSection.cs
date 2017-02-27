using GitHub.Models;
using System.Windows.Input;

namespace GitHub.VisualStudio.TeamExplorer.Connect
{
    public interface IGitHubConnectSection
    {
        void DoCreate();
        void SignOut();
        void Login();
        bool OpenRepository();
        IConnection SectionConnection { get; }
        ICommand Clone { get; }
    }
}
