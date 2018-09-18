using System;

namespace GitHub.Models
{
    public enum IssueState
    {
        Open,
        Closed,
    }

    /// <summary>
    /// Holds an overview of an issue for display in the issue list.
    /// </summary>
    public class IssueListItemModel
    {
        /// <summary>
        /// Gets or sets the GraphQL ID of the issue.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the issue number.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Gets or sets the issue author.
        /// </summary>
        public ActorModel Author { get; set; }

        /// <summary>
        /// Gets or sets the number of comments on the issue.
        /// </summary>
        public int CommentCount { get; set; }

        /// <summary>
        /// Gets or sets the issue title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the issue state (open, closed).
        /// </summary>
        public IssueState State { get; set; }

        /// <summary>
        /// Gets or sets the date/time at which the issue was last updated.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
