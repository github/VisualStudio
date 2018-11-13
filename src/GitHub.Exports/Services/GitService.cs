using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using GitHub.UI;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Extensions;
using LibGit2Sharp;

namespace GitHub.Services
{
    [Export(typeof(IGitService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitService : IGitService
    {
        readonly IRepositoryFacade repositoryFacade;

        [ImportingConstructor]
        public GitService(IRepositoryFacade repositoryFacade)
        {
            this.repositoryFacade = repositoryFacade;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalRepositoryModel"/> class.
        /// </summary>
        /// <param name="localPath">The repository's local path.</param>
        /// <returns>A repository model.</returns>
        public LocalRepositoryModel CreateLocalRepositoryModel(string localPath)
        {
            Guard.ArgumentNotNull(localPath, nameof(localPath));

            var dir = new DirectoryInfo(localPath);
            if (!dir.Exists)
            {
                throw new ArgumentException("Path does not exist", nameof(localPath));
            }

            using (var repository = GetRepository(localPath))
            {
                UriString cloneUrl = null;
                bool noOrigin = false;
                if (repository != null)
                {
                    cloneUrl = GetUri(repository);
                    noOrigin = HasRemotesButNoOrigin(repository);
                }

                var name = cloneUrl?.RepositoryName ?? dir.Name;

                var model = new LocalRepositoryModel
                {
                    LocalPath = localPath,
                    CloneUrl = cloneUrl,
                    HasRemotesButNoOrigin = noOrigin,
                    Name = name,
                    Icon = Octicon.repo
                };

                return model;
            }
        }

        public BranchModel GetBranch(LocalRepositoryModel model)
        {
            var localPath = model.LocalPath;
            using (var repo = GetRepository(localPath))
            {
                var branch = repo?.Head;
                if (branch == null)
                {
                    return null;
                }

                return new BranchModel(
                    name: branch.FriendlyName,
                    repo: model,
                    sha: branch.Tip?.Sha,
                    isTracking: branch.IsTracking,
                    trackedSha: branch.TrackedBranch?.Tip?.Sha,
                    trackedRemoteName: branch.TrackedBranch?.RemoteName);
            }
        }

        /// <summary>
        /// Returns the URL of the remote for the specified <see cref="repository"/>. If the repository
        /// is null or no remote named origin exists, this method returns null
        /// </summary>
        /// <param name="repository">The repository to look at for the remote.</param>
        /// <param name="remote">The name of the remote to look for</param>
        /// <returns>Returns a <see cref="UriString"/> representing the uri of the remote normalized to a GitHub repository url or null if none found.</returns>
        public UriString GetUri(IRepository repository, string remote = "origin")
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
        public UriString GetUri(string path, string remote = "origin")
        {
            using (var repo = GetRepository(path))
            {
                return GetUri(repo, remote);
            }
        }

        /// <summary>
        /// Probes for a git repository and if one is found, returns a <see cref="RepositoryModel"/> instance for the
        /// repository.
        /// </summary>
        /// <remarks>
        /// The lookup checks to see if the specified <paramref name="path"/> is a repository. If it's not, it then
        /// walks up the parent directories until it either finds a repository, or reaches the root disk.
        /// </remarks>
        /// <param name="path">The path to start probing</param>
        /// <returns>An instance of <see cref="RepositoryModel"/> or null</returns>
        public IRepository GetRepository(string path)
        {
            var repoPath = repositoryFacade.Discover(path);
            return repoPath == null ? null : repositoryFacade.NewRepository(repoPath);
        }

        /// <summary>
        /// Find out if repository has remotes but none are called "origin".
        /// </summary>
        /// <param name="repo">The target repository.</param>
        /// <returns>True if repository has remotes but none are called "origin".</returns>
        public bool HasRemotesButNoOrigin(IRepository repo)
        {
            var remotes = repo.Network.Remotes;
            return remotes["origin"] == null && remotes.Any();
        }

        /// <summary>
        /// Returns a <see cref="UriString"/> representing the uri of a remote
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="remote">The name of the remote to look for</param>
        /// <returns></returns>
        public UriString GetRemoteUri(IRepository repo, string remote = "origin")
        {
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
        public static IGitService GitServiceHelper => new GitService(new RepositoryFacade());

        /// <summary>
        /// Finds the latest pushed commit of a file and returns the sha of that commit. Returns null when no commits have 
        /// been found in any remote branches or the current local branch. 
        /// </summary>
        /// <param name="path">The local path of a repository or a file inside a repository. This cannot be null.</param>
        /// <param name="remote">The remote name to look for</param>
        /// <returns></returns>
        public Task<string> GetLatestPushedSha(string path, string remote = "origin")
        {
            Guard.ArgumentNotNull(path, nameof(path));

            return Task.Run(() =>
            {
                using (var repo = GetRepository(path))
                {
                    if (repo != null)
                    {
                        // This is the common case where HEAD is tracking a remote branch
                        var commonAncestor = repo.Head.TrackingDetails.CommonAncestor;
                        if (commonAncestor != null)
                        {
                            return commonAncestor.Sha;
                        }

                        // This is the common case where a branch was forked from a local branch.
                        // Use CommonAncestor because we don't want to search for a commit that only exists
                        // locally or that has been added to the remote tracking branch since the fork.
                        var commonAncestorShas = repo.Branches
                            .Where(b => b.IsTracking)
                            .Select(b => b.TrackingDetails.CommonAncestor?.Sha)
                            .Where(s => s != null)
                            .ToArray();

                        var sortByTopological = new CommitFilter { SortBy = CommitSortStrategies.Topological };
                        foreach (var commit in repo.Commits.QueryBy(sortByTopological))
                        {
                            if (commonAncestorShas.Contains(commit.Sha))
                            {
                                return commit.Sha;
                            }
                        }

                        // This is a less common case where a branch was forked from a branch
                        // which has since had new commits added to it.
                        var nearestCommonAncestor = repo.Branches
                            .Where(b => b.IsRemote)
                            .Select(b => b.Tip)
                            .Distinct()
                            .Select(c => repo.ObjectDatabase.CalculateHistoryDivergence(c, repo.Head.Tip))
                            .Where(hd => hd.AheadBy != null)
                            .OrderBy(hd => hd.BehindBy)
                            .Select(hd => hd.CommonAncestor)
                            .FirstOrDefault();
                        if (nearestCommonAncestor != null)
                        {
                            return nearestCommonAncestor.Sha;
                        }
                    }

                    return null;
                }
            });
        }
    }
}