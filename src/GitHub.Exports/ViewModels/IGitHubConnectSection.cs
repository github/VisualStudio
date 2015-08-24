using GitHub.Models;
using System.Collections.ObjectModel;

namespace GitHub.VisualStudio.TeamExplorer.Connect
{
    public interface IGitHubConnectSection
    {
        void DoCreate();
        void DoClone();
        void SignOut();
        void Login();
        void OpenRepository();
    }
}
