using System;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.ViewModels.Documents
{
    /// <summary>
    /// View model for an issue or pull request document pane.
    /// </summary>
    public interface IIssueishPaneViewModel : IPaneViewModel
    {
        /// <summary>
        /// Gets the content to display in the document pane.
        /// </summary>
        IViewModel Content { get; }

        /// <summary>
        /// Gets a value indicating whether <see cref="Load(IConnection, string, string, int)"/>
        /// has been called on the view model.
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Loads an issue or pull request into the view model.
        /// </summary>
        /// <param name="connection">The connection to use.</param>
        /// <param name="owner">The repository owner.</param>
        /// <param name="name">The repository name.</param>
        /// <param name="number">The issue or pull request number.</param>
        /// <returns>A task that will complete when the load has finished.</returns>
        Task Load(
            IConnection connection,
            string owner,
            string name,
            int number);
    }
}
