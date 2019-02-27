using System;
using GitHub.Extensions;
using GitHub.Services;

namespace GitHub.Models
{
    /// <summary>
    /// When attached as a property to a Visual Studio ITextBuffer, informs the inline comment
    /// tagger that the buffer represents a buffer opened from a pull request at the specified
    /// commit of a pull request.
    /// </summary>
    public class PullRequestTextBufferInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestTextBufferInfo"/> class.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="relativePath">The relative path to the file in the repository.</param>
        /// <param name="commitSha">The SHA of the commit.</param>
        /// <param name="side">Which side of a diff comparision the buffer represents.</param>
        public PullRequestTextBufferInfo(
            IPullRequestSession session,
            string relativePath,
            string commitSha,
            DiffSide? side)
        {
            Guard.ArgumentNotNull(session, nameof(session));
            Guard.ArgumentNotEmptyString(relativePath, nameof(relativePath));
            Guard.ArgumentNotEmptyString(commitSha, nameof(commitSha));

            Session = session;
            RelativePath = relativePath;
            CommitSha = commitSha;
            Side = side;
        }

        /// <summary>
        /// Gets the pull request session.
        /// </summary>
        public IPullRequestSession Session { get; }

        /// <summary>
        /// Gets the relative path to the file in the repository.
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// Gets the SHA of the commit.
        /// </summary>
        public string CommitSha { get; }

        /// <summary>
        /// Gets a value indicating which side of a diff comparision the buffer represents.
        /// </summary>
        public DiffSide? Side { get; }
    }
}
