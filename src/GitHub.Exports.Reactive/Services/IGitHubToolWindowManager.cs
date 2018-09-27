using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using GitHub.ViewModels.GitHubPane;
using GitHub.ViewModels.Documents;

namespace GitHub.Services
{
    /// <summary>
    /// The Visual Studio service interface for accessing the tool window panes.
    /// </summary>
    [Guid("FC9EC5B5-C297-4548-A229-F8E16365543C")]
    [ComVisible(true)]
    public interface IGitHubToolWindowManager
    {
        /// <summary>
        /// Ensure that the GitHub pane is created and visible.
        /// </summary>
        /// <returns>The view model for the GitHub Pane.</returns>
        Task<IGitHubPaneViewModel> ShowGitHubPane();

        /// <summary>
        /// Ensure that the GitHub pane is created and visible.
        /// </summary>
        /// <returns>The view model for the GitHub Pane.</returns>
        Task<IIssueDetailViewModel> ShowIssueDetailPane();
    }
}
