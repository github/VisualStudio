using System;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.Services
{
    /// <summary>
    /// Manages pull request sessions.
    /// </summary>
    public interface IPullRequestSessionManager
    {
        /// <summary>
        /// Gets an observable that tracks the current pull request session.
        /// </summary>
        /// <remarks>
        /// When first subscribed, this observable will fire immediately with the current pull
        /// request session or null if there is no current session. The current session is non-null
        /// if the current branch represents a pull request and changes when the current branch
        /// changes.
        /// </remarks>
        IObservable<IPullRequestSession> CurrentSession { get; }

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
