using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.VisualStudio
{
    public interface IServiceProviderPackage : IServiceProvider, Microsoft.VisualStudio.Shell.IAsyncServiceProvider
    {
    }

    [Guid("FC9EC5B5-C297-4548-A229-F8E16365543C")]
    [ComVisible(true)]
    public interface IGitHubToolWindowManager
    {
        Task<IGitHubPaneViewModel> ShowGitHubPane();
    }
}
