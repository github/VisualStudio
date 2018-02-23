using System;
using System.Threading.Tasks;
using GitHub.Collections;
using GitHub.Models;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Represents a view model for displaying the issues for a repository.
    /// </summary>
    public interface IIssueListViewModel : ISearchablePageViewModel, IOpenInBrowser
    {
        ITrackingCollection<IIssueListItemViewModel> Issues { get; }

        Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection);
    }
}
