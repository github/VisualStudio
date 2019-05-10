using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.CodeContainerManagement;

namespace GitHub.VisualStudio
{
    public interface IExtensionServices
    {
        ICodeContainerProvider GetGitHubContainerProvider();

        Task LoginAsync();
    }
}