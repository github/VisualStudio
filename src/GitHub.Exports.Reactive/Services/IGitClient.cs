using System;
using System.Reactive;
using LibGit2Sharp;

namespace GitHub.Services
{
    public interface IGitClient
    {
        /// <summary>
        /// Pushes the specified branch to the remote.
        /// </summary>
        /// <param name="repository">The repository to pull</param>
        /// <param name="remoteName">The name of the remote</param>
        /// <param name="branchName">the branch to pull</param>
        /// <returns></returns>
        IObservable<Unit> Push(IRepository repository, string branchName, string remoteName);

        /// <summary>
        /// Fetches the remote.
        /// </summary>
        /// <param name="repository">The repository to pull</param>
        /// <param name="remoteName">The name of the remote</param>
        /// <returns></returns>
        IObservable<Unit> Fetch(IRepository repository, string remoteName);

        /// <summary>
        /// Fetches the remote with a specific refspec.
        /// </summary>
        /// <param name="repository">The repository to pull</param>
        /// <param name="remoteName">The name of the remote</param>
        /// <param name="refSpec">The refspec to use</param>
        /// <returns></returns>
        IObservable<Unit> Fetch(IRepository repository, string remoteName, string refSpec);

        /// <summary>
        /// Sets the specified remote to the specified URL.
        /// </summary>
        /// <param name="repository">The repository to set</param>
        /// <param name="remoteName">The name of the remote</param>
        /// <param name="url">The URL to set as the remote</param>
        /// <returns></returns>
        IObservable<Unit> SetRemote(IRepository repository, string remoteName, Uri url);

        /// <summary>
        /// Sets the remote branch that the local branch tracks
        /// </summary>
        /// <param name="repository">The repository to set</param>
        /// <param name="branchName">The name of the remote</param>
        /// <param name="remoteName">The name of the branch (local and remote)</param>
        /// <returns></returns>
        IObservable<Unit> SetTrackingBranch(IRepository repository, string branchName, string remoteName);
    }
}
