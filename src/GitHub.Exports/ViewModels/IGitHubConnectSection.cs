using GitHub.Models;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GitHub.VisualStudio.TeamExplorer.Connect
{
    public interface IGitHubConnectSection
    {
        Task DoCreate();
        void SignOut();
        void Login();
        bool OpenRepository();
        IConnection SectionConnection { get; }
        ICommand Clone { get; }
    }
}
