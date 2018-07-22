using System;

namespace GitHub.Models
{
    /// <summary>
    /// Holds an overview of a pull request for display in the PR list.
    /// </summary>
    public class PullRequestListItemModel
    {
        /// <summary>
        /// Gets or sets the GraphQL ID of the pull request.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the pull request number.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Gets or sets the pull request author.
        /// </summary>
        public ActorModel Author { get; set; }

        /// <summary>
        /// Gets or sets the number of comments on the pull request.
        /// </summary>
        public int CommentCount { get; set; }

        /// <summary>
        /// Gets or sets the pull request title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the pull request state (open, closed, merged).
        /// </summary>
        public PullRequestStateEnum State { get; set; }

        /// <summary>
        /// Gets or sets the date/time at which the pull request was last updated.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
