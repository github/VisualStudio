using System;
using System.ComponentModel;
using System.Threading.Tasks;
using GitHub.Models;

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
    }
}
