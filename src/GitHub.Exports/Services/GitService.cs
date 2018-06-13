using System.ComponentModel.Composition;
using GitHub.Primitives;
using LibGit2Sharp;
using System;
using System.Threading.Tasks;
using GitHub.Models;
using System.Linq;
using GitHub.Extensions;

namespace GitHub.Services
{
    [Export(typeof(IGitService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitService : IGitService
    {
        const string defaultOriginName = "origin";

        /// <summary>
        /// Returns the URL of the remote for the specified <see cref="repository"/>. If the repository
        /// is null or no remote named origin exists, this method returns null
        /// </summary>
        /// <param name="repository">The repository to look at for the remote.</param>
        /// <param name="remote">The name of the remote to look for</param>
        /// <returns>Returns a <see cref="UriString"/> representing the uri of the remote normalized to a GitHub repository url or null if none found.</returns>
        public UriString GetUri(IRepository repository, string remote = null)
        {
            return UriString.ToUriString(GetRemoteUri(repository, remote)?.ToRepositoryUrl());
        }

        /// <summary>
        /// Probes for a git repository and if one is found, returns a normalized GitHub uri <see cref="UriString"/>
        /// for the repository's remote if one is found
        /// </summary>
        /// <remarks>
        /// The lookup checks to see if the specified <paramref name="path"/> is a repository. If it's not, it then
        /// walks up the parent directories until it either finds a repository, or reaches the root disk.
        /// </remarks>
        /// <param name="path">The path to start probing</param>
        /// <param name="remote">The name of the remote to look for</param>
        /// <returns>Returns a <see cref="UriString"/> representing the uri of the remote normalized to a GitHub repository url or null if none found.</returns>
        public UriString GetUri(string path, string remote = null)
        {
            using (var repo = GetRepository(path))
            {
                return GetUri(repo, remote);
            }
        }

        /// <summary>
        /// Probes for a git repository and if one is found, returns a <see cref="IRepositoryModel"/> instance for the
        /// repository.
        /// </summary>
        /// <remarks>
        /// The lookup checks to see if the specified <paramref name="path"/> is a repository. If it's not, it then
        /// walks up the parent directories until it either finds a repository, or reaches the root disk.
        /// </remarks>
        /// <param name="path">The path to start probing</param>
        /// <returns>An instance of <see cref="IRepositoryModel"/> or null</returns>
        public IRepository GetRepository(string path)
        {
            var repoPath = Repository.Discover(path);
            return repoPath == null ? null : new Repository(repoPath);
        }

        /// <summary>
        /// Returns a <see cref="UriString"/> representing the uri of a remote
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="remote">The name of the remote to look for</param>
        /// <returns></returns>
        public UriString GetRemoteUri(IRepository repo, string remote = null)
        {
            if (repo == null)
            {
                return null;
            }

            remote = remote ?? FindOriginalRemoteName(repo);

            return repo
                ?.Network
                .Remotes[remote]
                ?.Url;
        }

        /// <summary>
        /// Get a new instance of <see cref="GitService"/>. 
        /// </summary>
        /// <remarks>
        /// This is equivalent to creating it via MEF with <see cref="CreationPolicy.NonShared"/>
        /// </remarks>
        public static IGitService GitServiceHelper => new GitService();

        /// <summary>
        /// Finds the latest pushed commit of a file and returns the sha of that commit. Returns null when no commits have 
        /// been found in any remote branches or the current local branch. 
        /// </summary>
        /// <param name="path">The local path of a repository or a file inside a repository. This cannot be null.</param>
        /// <returns></returns>
        public Task<string> GetLatestPushedSha(string path)
        {
            Guard.ArgumentNotNull(path, nameof(path));

            return Task.Factory.StartNew(() =>
            {
                using (var repo = GetRepository(path))
                {
                    if (repo != null)
                    {
                        if (repo.Head.IsTracking && repo.Head.Tip.Sha == repo.Head.TrackedBranch.Tip.Sha)
                        {
                            return repo.Head.Tip.Sha;
                        }

                        var remoteHeads = repo.Refs.Where(r => r.IsRemoteTrackingBranch).ToList();
                        foreach (var c in repo.Commits)
                        {
                            if (repo.Refs.ReachableFrom(remoteHeads, new[] { c }).Any())
                            {
                                return c.Sha;
                            }
                        }
                    }

                    return null;
                }
            });
        }

        /// <summary>
        /// Find a remote named "origin" or the first remote in the list.
        /// </summary>
        /// <remarks>
        /// When a repository is cloned, a remote named "origin" is automatically added with the clone URL. 
        /// The remote list order doesn't change when a remote is added, deleted or renamed. This means that
        /// if a user renames "origin", the first remote in the list will still contain the original clone URL.
        /// </remarks>
        /// <param name="repo">The <see cref="IRepository" /> to find a remote for.</param>
        /// <returns>The remote named "origin" or the first remote in the list.</returns>
        public string FindOriginalRemoteName(IRepository repo)
        {
            var remotes = repo.Network.Remotes;
            var remote = remotes[defaultOriginName];
            if (remote != null)
            {
                return remote.Name;
            }

            remote = remotes.FirstOrDefault();
            if (remote != null)
            {
                return remote.Name;
            }

            return null;
        }
    }
}
