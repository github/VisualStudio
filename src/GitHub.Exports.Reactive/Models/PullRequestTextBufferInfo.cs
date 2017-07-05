using System;
using GitHub.Services;

namespace GitHub.Models
{
    /// <summary>
    /// When attached as a property to a Visual Studio ITextBuffer, informs the inline comment
    /// tagger that the buffer represents a buffer opened from a pull request.
    /// </summary>
    public class PullRequestTextBufferInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestTextBufferInfo"/> class.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="filePath">The full path to the file.</param>
        /// <param name="isLeftComparisonBuffer">
        /// Whether the buffer represents the left-hand-side of a comparison.
        /// </param>
        public PullRequestTextBufferInfo(
            IPullRequestSession session,
            string filePath,
            bool isLeftComparisonBuffer)
        {
            Session = session;
            FilePath = filePath;
            IsLeftComparisonBuffer = isLeftComparisonBuffer;
        }

        /// <summary>
        /// Gets the pull request session.
        /// </summary>
        public IPullRequestSession Session { get; }

        /// <summary>
        /// Gets the full path to the file.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Gets a value indicating whether the buffer represents the left-hand-side of a comparison.
        /// </summary>
        public bool IsLeftComparisonBuffer { get; }
    }
}
