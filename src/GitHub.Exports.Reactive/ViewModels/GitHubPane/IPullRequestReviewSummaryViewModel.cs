using GitHub.Models;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Displays a short overview of a pull request review in the <see cref="IPullRequestDetailViewModel"/>.
    /// </summary>
    public interface IPullRequestReviewSummaryViewModel
    {
        /// <summary>
        /// Gets the ID of the pull request review.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets the user who submitted the review.
        /// </summary>
        IActorViewModel User { get; set; }

        /// <summary>
        /// Gets the state of the review.
        /// </summary>
        PullRequestReviewState State { get; set; }

        /// <summary>
        /// Gets a string representing the state of the review.
        /// </summary>
        string StateDisplay { get; }

        /// <summary>
        /// Gets the number of file comments in the review.
        /// </summary>
        int FileCommentCount { get; set; }
    }
}