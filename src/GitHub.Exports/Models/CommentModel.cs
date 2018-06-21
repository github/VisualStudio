using System;

namespace GitHub.Models
{
    /// <summary>
    /// An issue or pull request review comment.
    /// </summary>
    public class CommentModel
    {
        /// <summary>
        /// Gets the ID of the comment.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets the author of the comment.
        /// </summary>
        public ActorModel Author { get; set; }

        /// <summary>
        /// Gets the body of the comment.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets the creation time of the comment.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets the HTTP URL permalink for the comment.
        /// </summary>
        public string Url { get; set; }
    }
}
