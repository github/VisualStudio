using System;
using GitHub.Services;

namespace GitHub.Models
{
    /// <summary>
    /// When attached as a property to a Visual Studio ITextBuffer, informs the inline comment
    /// tagger that the buffer represents a buffer opened from a pull request at the HEAD commit
    /// of a pull request.
    /// </summary>
    public class PullRequestTextBufferInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestTextBufferInfo"/> class.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="relativePath">The relative path to the file in the repository.</param>
        /// <param name="side">Which side of a diff comparision the buffer represents.</param>
        public PullRequestTextBufferInfo(
            IPullRequestSession session,
            string relativePath,
            DiffSide? side)
        {
            Session = session;
            RelativePath = relativePath;
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
        /// Gets a value indicating which side of a diff comparision the buffer represents.
        /// </summary>
        public DiffSide? Side { get; }
    }
}
