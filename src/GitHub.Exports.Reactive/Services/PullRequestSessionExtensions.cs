using System;
using System.Linq;
using GitHub.Extensions;

namespace GitHub.Services
{
    /// <summary>
    /// Extension methods for <see cref="IPullRequestSession"/>.
    /// </summary>
    public static class PullRequestSessionExtensions
    {
        /// <summary>
        /// Gets the head (source) branch label for a pull request, stripping the owner if the pull
        /// request is not from a fork.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <returns>The head branch label</returns>
        public static string GetHeadBranchDisplay(this IPullRequestSession session)
        {
            Guard.ArgumentNotNull(session, nameof(session));
            return GetBranchDisplay(session.IsPullRequestFromFork(), session.PullRequest?.Head?.Label);
        }

        /// <summary>
        /// Gets the head (target) branch label for a pull request, stripping the owner if the pull
        /// request is not from a fork.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <returns>The head branch label</returns>
        public static string GetBaseBranchDisplay(this IPullRequestSession session)
        {
            Guard.ArgumentNotNull(session, nameof(session));
            return GetBranchDisplay(session.IsPullRequestFromFork(), session.PullRequest?.Base?.Label);
        }

        /// <summary>
        /// Returns a value that determines whether the pull request comes from a fork.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <returns>True if the pull request is from a fork, otherwise false.</returns>
        public static bool IsPullRequestFromFork(this IPullRequestSession session)
        {
            Guard.ArgumentNotNull(session, nameof(session));

            var headUrl = session.PullRequest.Head.RepositoryCloneUrl?.ToRepositoryUrl();
            var localUrl = session.LocalRepository.CloneUrl?.ToRepositoryUrl();
            return headUrl != null && localUrl != null ? headUrl != localUrl : false;
        }

        static string GetBranchDisplay(bool fork, string label)
        {
            if (label != null)
            {
                return fork ? label : label.Split(':').Last();
            }

            return "[invalid]";
        }
    }
}
