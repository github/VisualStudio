using System;
using GitHub.Primitives;
using GitHub.Models;
using Microsoft.VisualStudio.Threading;

namespace GitHub.Services
{
    /// <summary>
    /// Used by the home section to share its IServiceProvider
    /// with the nav items (which otherwise would not have access to one
    /// with source control change events)
    /// </summary>
    public interface ITeamExplorerServiceHolder
    {
        /// <summary>
        /// A IServiceProvider that provides an ITeamFoundationContextManager
        /// service that can raise ContextChanged events when something
        /// changes in the source control context.
        /// </summary>
        IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Clears the current ServiceProvider if it matches the one that is passed in.
        /// This is usually called on Dispose, which might happen after another section
        /// has changed the ServiceProvider to something else, which is why we require
        /// the parameter to match.
        /// </summary>
        /// <param name="provider">If the current ServiceProvider matches this, clear it</param>
        void ClearServiceProvider(IServiceProvider provider);

        /// <summary>
        /// A service that can be used for repository changed events.
        /// </summary>
        ITeamExplorerContext TeamExplorerContext { get; }

        /// <summary>
        /// A service for avoiding deadlocks and marshaling tasks onto the UI thread.
        /// </summary>
        JoinableTaskFactory JoinableTaskFactory { get; }

        IGitAwareItem HomeSection { get; }
    }

    public interface IGitAwareItem
    {
        LocalRepositoryModel ActiveRepo { get; }

        /// <summary>
        /// Represents the web URL of the repository on GitHub.com, even if the origin is an SSH address.
        /// </summary>
        UriString ActiveRepoUri { get; }
        string ActiveRepoName { get; }
    }
}
