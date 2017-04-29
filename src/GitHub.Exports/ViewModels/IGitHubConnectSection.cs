using GitHub.Models;
using System.Threading.Tasks;
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

namespace GitHub.VisualStudio.TeamExplorer.Sync
{
    public interface ISynchronizeForkWithUpstreamSection
    {
        Task Synchronize();
    }
}
