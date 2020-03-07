using Microsoft.VisualStudio.Shell.CodeContainerManagement;

namespace GitHub.VisualStudio
{
    public interface IExtensionServices
    {
        ICodeContainerProvider GetGitHubContainerProvider();
    }
}