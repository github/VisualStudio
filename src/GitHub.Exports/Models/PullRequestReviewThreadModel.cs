using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    /// <summary>
    /// Represents a thread of <see cref="PullRequestReviewCommentModel"/>s.
    /// </summary>
    public class PullRequestReviewThreadModel
    {
        /// <summary>
        /// Gets or sets the GraphQL ID of the thread.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the path to the file that the thread is on, relative to the repository.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the SHA of the commmit that the thread starts on.
        /// </summary>
        public string CommitSha { get; set; }

        /// <summary>
        /// Gets or sets the diff hunk for the thread.
        /// </summary>
        public string DiffHunk { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the thread is outdated.
        /// </summary>
        public bool IsOutdated { get; set; }

        /// <summary>
        /// Gets or sets the line position in the diff that the thread starts on.
        /// </summary>
        /// <remarks>
        /// This property reflects the <see cref="OriginalPosition"/> updated for the current
        /// <see cref="CommitSha"/>. If the thread is outdated, it will return null.
        /// </remarks>
        public int? Position { get; set; }

        /// <summary>
        /// Gets or sets the line position in the diff that the thread was originally started on.
        /// </summary>
        /// <remarks>
        /// This property represents a line in the diff between the <see cref="OriginalCommitSha"/>
        /// and the pull request branch's merge base at which the thread was originally started.
        /// </remarks>
        public int OriginalPosition { get; set; }

        /// <summary>
        /// Gets or sets the SHA of the commmit that the thread was originally started on.
        /// </summary>
        public string OriginalCommitSha { get; set; }

        /// <summary>
        /// Gets or sets the comments in the thread.
        /// </summary>
        public IReadOnlyList<PullRequestReviewCommentModel> Comments { get; set; }
    }
}
