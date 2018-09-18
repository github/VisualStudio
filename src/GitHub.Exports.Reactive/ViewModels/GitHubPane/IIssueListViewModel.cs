using System;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Represents a view model which displays an issue list.
    /// </summary>
    public interface IIssueListViewModel : IIssueListViewModelBase, IOpenInBrowser
    {
        /// <summary>
        /// Gets a command that opens an issue on GitHub.
        /// </summary>
        ReactiveCommand<object> OpenItemInBrowser { get; }
    }
}
