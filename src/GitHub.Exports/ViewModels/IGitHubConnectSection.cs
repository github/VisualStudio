using GitHub.Models;
using System.Collections.ObjectModel;

namespace GitHub.VisualStudio.TeamExplorer.Connect
{
    public interface IGitHubConnectSection
    {
        ObservableCollection<ISimpleRepositoryModel> Repositories { get; set; }

        void DoCreate();
        void DoClone();
        void SignOut();
        void Login();
    }
}
