using System;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Describes the ways that the display of <see cref="IGitHubPaneViewModel.Content"/> can
    /// be overridden.
    /// </summary>
    public enum ContentOverride
    {
        /// <summary>
        /// No override, display the content.
        /// </summary>
        None,

        /// <summary>
        /// Display a spinner instead of the content.
        /// </summary>
        Spinner,

        /// <summary>
        /// Display an error instead of the content.
        /// </summary>
        Error
    }

    /// <summary>
    /// The view model for the GitHub Pane.
    /// </summary>
    public interface IGitHubPaneViewModel : IViewModel
    {
        /// <summary>
        /// Gets the connection to the current repository.
        /// </summary>
        IConnection Connection { get; }

        /// <summary>
        /// Gets the content to display in the GitHub pane.
        /// </summary>
        IViewModel Content { get; }

        /// <summary>
        /// Gets a value describing whether to display the <see cref="Content"/> or to override
        /// it with another view.
        /// </summary>
        ContentOverride ContentOverride { get; }

        /// <summary>
        /// Gets a value indicating whether search is available on the current page.
        /// </summary>
        bool IsSearchEnabled { get; }

        /// <summary>
        /// Gets the local repository.
        /// </summary>
        LocalRepositoryModel LocalRepository { get; }

        /// <summary>
        /// Gets or sets the search query for the current page.
        /// </summary>
        string SearchQuery { get; set; }

        /// <summary>
        /// Gets the title to display in the GitHub pane header.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        Task InitializeAsync(IServiceProvider paneServiceProvider);

        /// <summary>
        /// Navigates to a GitHub Pane URL.
        /// </summary>
        /// <param name="uri">The URL.</param>
        Task NavigateTo(Uri uri);

        /// <summary>
        /// Shows the pull reqest list in the GitHub pane.
        /// </summary>
        Task ShowPullRequests();

        /// <summary>
        /// Shows the details for a pull request in the GitHub pane.
        /// </summary>
        /// <param name="owner">The repository owner.</param>
        /// <param name="repo">The repository name.</param>
        /// <param name="number">The pull rqeuest number.</param>
        Task ShowPullRequest(string owner, string repo, int number);

        /// <summary>
        /// Shows the details for a pull request's check run in the GitHub pane.
        /// </summary>
        /// <param name="owner">The repository owner.</param>
        /// <param name="repo">The repository name.</param>
        /// <param name="number">The pull rqeuest number.</param>
        /// <param name="checkRunId">The check run id.</param>
        Task ShowPullRequestCheckRun(string owner, string repo, int number, string checkRunId);

        /// <summary>
        /// Shows the pull requests reviews authored by a user.
        /// </summary>
        /// <param name="owner">The repository owner.</param>
        /// <param name="repo">The repository name.</param>
        /// <param name="number">The pull rqeuest number.</param>
        /// <param name="login">The user login.</param>
        Task ShowPullRequestReviews(string owner, string repo, int number, string login);

        /// <summary>
        /// Shows a pane authoring a pull request review.
        /// </summary>
        /// <param name="owner">The repository owner.</param>
        /// <param name="repo">The repository name.</param>
        /// <param name="number">The pull rqeuest number.</param>
        Task ShowPullRequestReviewAuthoring(string owner, string repo, int number);
    }
}