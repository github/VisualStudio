using System;
using System.ComponentModel;
using System.Threading.Tasks;
using GitHub.Models;
using Microsoft.VisualStudio.Text;

namespace GitHub.Services
{
    /// <summary>
    /// Manages pull request sessions.
    /// </summary>
    public interface IPullRequestSessionManager : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the current pull request session.
        /// </summary>
        /// <returns>
        /// The current pull request session, or null if the currently checked out branch is not
        /// a pull request branch.
        /// </returns>
        IPullRequestSession CurrentSession { get; }

        /// <summary>
        /// Gets a pull request session for a pull request that may not be checked out.
        /// </summary>
        /// <param name="pullRequest">The pull request model.</param>
        /// <returns>An <see cref="IPullRequestSession"/>.</returns>
        /// <remarks>
        /// If the provided pull request model represents the current session then that will be
        /// returned. If not, a new pull request session object will be created.
        /// </remarks>
        Task<IPullRequestSession> GetSession(IPullRequestModel pullRequest);

        /// <summary>
        /// Gets information about the pull request that a Visual Studio text buffer is a part of.
        /// </summary>
        /// <param name="buffer">The text buffer.</param>
        /// <returns>
        /// A <see cref="PullRequestTextBufferInfo"/> or null if the pull request for the text
        /// buffer could not be determined.
        /// </returns>
        /// <remarks>
        /// This method looks for a <see cref="PullRequestTextBufferInfo"/> object stored in the text
        /// buffer's properties.
        /// </remarks>
        PullRequestTextBufferInfo GetTextBufferInfo(ITextBuffer buffer);
    }
}
