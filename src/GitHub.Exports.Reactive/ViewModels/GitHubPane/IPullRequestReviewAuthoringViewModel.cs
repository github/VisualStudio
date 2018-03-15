using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Represents a view model for displaying details of a pull request review that is being
    /// authored.
    /// </summary>
    public interface IPullRequestReviewAuthoringViewModel : IPullRequestReviewViewModelBase, IPanePageViewModel, IDisposable
    {
        /// <summary>
        /// Gets or sets the body of the pull request review to be submitted.
        /// </summary>
        string Body { get; }

        /// <summary>
        /// Gets the pull request's changed files.
        /// </summary>
        IPullRequestFilesViewModel Files { get; }

        /// <summary>
        /// Gets a list of the file comments in the review.
        /// </summary>
        IReadOnlyList<IPullRequestReviewFileCommentViewModel> FileComments { get; }

        /// <summary>
        /// Gets a command which navigates to the parent pull request.
        /// </summary>
        ReactiveCommand<object> NavigateToPullRequest { get; }

        /// <summary>
        /// Gets a command which submits the review.
        /// </summary>
        ReactiveCommand<Unit> Submit { get; }

        /// <summary>
        /// Initializes the view model for creating a new review.
        /// </summary>
        /// <param name="localRepository">The local repository.</param>
        /// <param name="connection">The connection to the repository host.</param>
        /// <param name="owner">The pull request's repository owner.</param>
        /// <param name="repo">The pull request's repository name.</param>
        /// <param name="pullRequestNumber">The pull request number.</param>
        Task InitializeAsync(
            ILocalRepositoryModel localRepository,
            IConnection connection,
            string owner,
            string repo,
            int pullRequestNumber);

        /// <summary>
        /// Updates the data from a pull request model.
        /// </summary>
        /// <param name="pullRequest">The pull request model.</param>
        /// <returns>A task tracking the operation.</returns>
        Task Load(IPullRequestModel pullRequest);
    }
}
