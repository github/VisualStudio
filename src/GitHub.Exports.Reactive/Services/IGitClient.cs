using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
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
        /// Fetches from the remote, using custom refspecs.
        /// </summary>
        /// <param name="repository">The repository to pull</param>
        /// <param name="remoteName">The name of the remote</param>
        /// <param name="refspecs">The custom refspecs</param>
        /// <returns></returns>
        IObservable<Unit> Fetch(IRepository repository, string remoteName, params string[] refspecs);

        /// <summary>
        /// Checks out a branch.
        /// </summary>
        /// <param name="repository">The repository to checkout to.</param>
        /// <param name="branch">The name of the branch</param>
        /// <returns></returns>
        IObservable<Unit> Checkout(IRepository repository, string branchName);

        /// <summary>
        /// Merges a ref into the current branch.
        /// </summary>
        /// <param name="repository">The repository</param>
        /// <param name="refToMerge">The ref to merge into the current branch</param>
        /// <param name="signature">The signature of who is performing the merge</param>
        /// <returns></returns>
        IObservable<Unit> Merge(IRepository repository, string refToMerge);

        /// <summary>
        /// Carries out a pull on the current branch.
        /// </summary>
        /// <returns></returns>
        IObservable<Unit> Pull(IRepository repository);

        /// <summary>
        /// Gets all local branches whose name starts with the specified string.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="s">The string.</param>
        /// <returns></returns>
        IObservable<Branch> GetBranchStartsWith(IRepository repository, string s);

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

        IObservable<Remote> GetHttpRemote(IRepository repo, string remote);
    }
}
 