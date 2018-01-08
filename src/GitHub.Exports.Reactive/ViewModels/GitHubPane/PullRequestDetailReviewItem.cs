using System;
using GitHub.Models;
using Octokit;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Displays a short overview of a pull request review in the pull rqeuest detail pane.
    /// </summary>
    public class PullRequestDetailReviewItem
    {
        readonly PullRequestReviewState state;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestDetailReviewItem"/> class.
        /// </summary>
        /// <param name="id">The ID of the pull request review.</param>
        /// <param name="user">The user who submitted the review.</param>
        /// <param name="state">The state of the review.</param>
        /// <param name="fileCommentCount">The number of file comments in the review.</param>
        public PullRequestDetailReviewItem(
            long id,
            IAccount user,
            PullRequestReviewState state,
            int fileCommentCount)
        {
            Id = id;
            User = user;
            this.state = state;
            FileCommentCount = fileCommentCount;
        }

        /// <summary>
        /// Gets the ID of the pull request review.
        /// </summary>
        public long Id { get; }

        /// <summary>
        /// Gets the user who submitted the review.
        /// </summary>
        public IAccount User { get; }

        /// <summary>
        /// Gets a string representing the state of the review.
        /// </summary>
        public string State => ToString(state);

        /// <summary>
        /// Gets the name of the icon representation of the state.
        /// </summary>
        public string Icon => ToIcon(state);

        /// <summary>
        /// Gets the number of file comments in the review.
        /// </summary>
        public int FileCommentCount { get; }

        static string ToString(PullRequestReviewState state)
        {
            switch (state)
            {
                case PullRequestReviewState.Approved:
                    return "approved";
                case PullRequestReviewState.ChangesRequested:
                    return "requested changes";
                case PullRequestReviewState.Commented:
                    return "commented";
                default:
                    throw new NotSupportedException();
            }
        }

        static string ToIcon(PullRequestReviewState state)
        {
            switch (state)
            {
                case PullRequestReviewState.Approved:
                    return "check";
                case PullRequestReviewState.ChangesRequested:
                    return "x";
                case PullRequestReviewState.Commented:
                    return "comment";
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
