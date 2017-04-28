using System;
using GitHub.Primitives;
using GitHub.Models;

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
        /// A IGitRepositoryInfo representing the currently active repository
        /// </summary>
        ILocalRepositoryModel ActiveRepo { get; }
        /// <summary>
        /// Subscribe to be notified when the active repository is set and Notify is called.
        /// </summary>
        /// <param name="who">The instance that is interested in being called (or a unique key/object for that instance)</param>
        /// <param name="handler">The handler to call when ActiveRepo is set</param>
        void Subscribe(object who, Action<ILocalRepositoryModel> handler);
        /// <summary>
        /// Unsubscribe from notifications
        /// </summary>
        /// <param name="who">The instance/key that previously subscribed to notifications</param>
        void Unsubscribe(object who);

        IGitAwareItem HomeSection { get; }

        /// <summary>
        /// Refresh the information on the active repo (in case of remote url changes or other such things)
        /// </summary>
        void Refresh();
    }

    public interface IGitAwareItem
    {
        ILocalRepositoryModel ActiveRepo { get; }

        /// <summary>
        /// Represents the web URL of the repository on GitHub.com, even if the origin is an SSH address.
        /// </summary>
        UriString ActiveRepoUri { get; }
        string ActiveRepoName { get; }
    }
}
