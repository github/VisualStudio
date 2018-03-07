using System;
using System.Windows.Media;
using GitHub.Models;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Displays a short overview of a pull request review in the pull rqeuest detail pane.
    /// </summary>
    public class PullRequestDetailReviewItem
    {
        /// <summary>
        /// Gets the ID of the pull request review.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets the user who submitted the review.
        /// </summary>
        public IAccount User { get; set; }

        /// <summary>
        /// Gets the state of the review.
        /// </summary>
        public PullRequestReviewState State { get; set; }

        /// <summary>
        /// Gets a string representing the state of the review.
        /// </summary>
        public string StateDisplay => ToString(State);

        /// <summary>
        /// Gets the number of file comments in the review.
        /// </summary>
        public int FileCommentCount { get; set; }

        /// <summary>
        /// Gets the string representation of a <see cref="PullRequestReviewState"/>
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>The string representation.</returns>
        public static string ToString(PullRequestReviewState state)
        {
            switch (state)
            {
                case PullRequestReviewState.Approved:
                    return "Approved";
                case PullRequestReviewState.ChangesRequested:
                    return "Changes requested";
                case PullRequestReviewState.Commented:
                case PullRequestReviewState.Dismissed:
                    return "Commented";
                case PullRequestReviewState.Pending:
                    return "In progress";
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
