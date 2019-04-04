using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using GitHub.ViewModels.GitHubPane;
using GitHub.ViewModels.Documents;
using GitHub.Primitives;

namespace GitHub.Services
{
    /// <summary>
    /// The Visual Studio service interface for accessing the GitHub Pane.
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
        /// Shows a document-like tool window pane for an issue or pull request.
        /// </summary>
        /// <param name="address">
        /// The host address of the server that hosts the issue or pull request.
        /// </param>
        /// <param name="owner">The repository owner.</param>
        /// <param name="repository">The repository name.</param>
        /// <param name="number">The issue or pull request number.</param>
        /// <returns>The view model for the document pane.</returns>
        Task<IIssueishPaneViewModel> ShowIssueishDocumentPane(
            HostAddress address,
            string owner,
            string repository,
            int number);
    }
}
