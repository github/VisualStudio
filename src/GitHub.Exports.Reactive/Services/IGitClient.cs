using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Models;
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
        Task Push(IRepository repository, string branchName, string remoteName);

        /// <summary>
        /// Fetches the remote.
        /// </summary>
        /// <param name="repository">The repository to pull</param>
        /// <param name="remoteName">The name of the remote</param>
        /// <returns></returns>
        Task Fetch(IRepository repository, string remoteName);

        /// <summary>
        /// Fetches from the remote, using custom refspecs.
        /// </summary>
        /// <param name="repository">The repository to pull</param>
        /// <param name="remoteName">The name of the remote</param>
        /// <param name="refspecs">The custom refspecs</param>
        /// <returns></returns>
        Task Fetch(IRepository repository, string remoteName, params string[] refspecs);

        /// <summary>
        /// Checks out a branch.
        /// </summary>
        /// <param name="repository">The repository to carry out the checkout on</param>
        /// <param name="branchName">The name of the branch</param>
        /// <returns></returns>
        Task Checkout(IRepository repository, string branchName);

        /// <summary>
        /// Sets the configuration key to the specified value in the local config.
        /// </summary>
        /// <param name="repository">The repository</param>
        /// <param name="key">The configuration key. Keys are in the form 'section.name'.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        Task SetConfig(IRepository repository, string key, string value);

        /// <summary>
        /// Sets the specified remote to the specified URL.
        /// </summary>
        /// <param name="repository">The repository to set</param>
        /// <param name="remoteName">The name of the remote</param>
        /// <param name="url">The URL to set as the remote</param>
        /// <returns></returns>
        Task SetRemote(IRepository repository, string remoteName, Uri url);

        /// <summary>
        /// Sets the remote branch that the local branch tracks
        /// </summary>
        /// <param name="repository">The repository to set</param>
        /// <param name="branchName">The name of the local remote</param>
        /// <param name="remoteName">The name of the remote branch</param>
        /// <returns></returns>
        Task SetTrackingBranch(IRepository repository, string branchName, string remoteName);

        Task<Remote> GetHttpRemote(IRepository repo, string remote);
    }
}
