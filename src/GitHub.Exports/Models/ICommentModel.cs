using System;

namespace GitHub.Models
{
    /// <summary>
    /// An issue or pull request review comment.
    /// </summary>
    public interface ICommentModel
    {
        /// <summary>
        /// Gets the ID of the comment.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets the author of the comment.
        /// </summary>
        IAccount User { get; }

        /// <summary>
        /// Gets the body of the comment.
        /// </summary>
        string Body { get; }

        /// <summary>
        /// Gets the last updated time of the comment.
        /// </summary>
        DateTimeOffset UpdatedAt { get; }
    }
}
