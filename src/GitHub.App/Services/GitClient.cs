using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using LibGit2Sharp;
using GitHub.Logging;
using Serilog;

namespace GitHub.Services
{
    [Export(typeof(IGitClient))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GitClient : IGitClient
    {
        static readonly ILogger log = LogManager.ForContext<GitClient>();
        readonly IGitService gitService;
        readonly PullOptions pullOptions;
        readonly PushOptions pushOptions;
        readonly FetchOptions fetchOptions;

        [ImportingConstructor]
        public GitClient(IGitHubCredentialProvider credentialProvider, IGitService gitService)
        {
            Guard.ArgumentNotNull(credentialProvider, nameof(credentialProvider));
            Guard.ArgumentNotNull(gitService, nameof(gitService));

            this.gitService = gitService;

            pushOptions = new PushOptions { CredentialsProvider = credentialProvider.HandleCredentials };
            fetchOptions = new FetchOptions { CredentialsProvider = credentialProvider.HandleCredentials };
            pullOptions = new PullOptions
            {
                FetchOptions = fetchOptions,
                MergeOptions = new MergeOptions(),
            };
        }

        public Task Pull(IRepository repository)
        {
            Guard.ArgumentNotNull(repository, nameof(repository));
            return Task.Run(() =>
            {
                var signature = repository.Config.BuildSignature(DateTimeOffset.UtcNow);
                if (repository is Repository repo)
                {
                    LibGit2Sharp.Commands.Pull(repo, signature, pullOptions);
                }
                else
                {
                    log.Error("Couldn't pull because {Variable} isn't an instance of {Type}", nameof(repository), typeof(Repository));
                }
            });
        }

        public Task Push(IRepository repository, string branchName, string remoteName)
        {
            Guard.ArgumentNotNull(repository, nameof(repository));
            Guard.ArgumentNotEmptyString(branchName, nameof(branchName));
            Guard.ArgumentNotEmptyString(remoteName, nameof(remoteName));

            return Task.Run(() =>
            {
                if (repository.Head?.Commits != null && repository.Head.Commits.Any())
                {
                    var remote = repository.Network.Remotes[remoteName];
                    var remoteRef = IsCanonical(branchName) ? branchName : @"refs/heads/" + branchName;
                    repository.Network.Push(remote, "HEAD", remoteRef, pushOptions);
                }
            });
        }

        public Task Fetch(IRepository repo, UriString cloneUrl, params string[] refspecs)
        {
            foreach (var remote in repo.Network.Remotes)
            {
                if (UriString.RepositoryUrlsAreEqual(new UriString(remote.Url), cloneUrl))
                {
                    return Fetch(repo, remote.Name, refspecs);
                }
            }

            return Task.Run(() =>
            {
                try
                {
                    var remoteName = cloneUrl.Owner;
                    var remoteUri = cloneUrl.ToRepositoryUrl();

                    var removeRemote = false;
                    if (repo.Network.Remotes[remoteName] != null)
                    {
                        // If a remote with this name already exists, use a unique name and remove remote afterwards
                        remoteName = cloneUrl.Owner + "-" + Guid.NewGuid();
                        removeRemote = true;
                    }

                    repo.Network.Remotes.Add(remoteName, remoteUri.ToString());
                    try
                    {
                        repo.Network.Fetch(remoteName, refspecs, fetchOptions);
                    }
                    finally
                    {
                        if (removeRemote)
                        {
                            repo.Network.Remotes.Remove(remoteName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Failed to fetch");
#if DEBUG
                    throw;
#endif
                }
            });
        }

        public Task Fetch(IRepository repository, string remoteName, params string[] refspecs)
        {
            Guard.ArgumentNotNull(repository, nameof(repository));
            Guard.ArgumentNotEmptyString(remoteName, nameof(remoteName));

            return Task.Run(() =>
            {
                try
                {
                    repository.Network.Fetch(remoteName, refspecs, fetchOptions);
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Failed to fetch");
#if DEBUG
                    throw;
#endif
                }
            });
        }

        public Task Checkout(IRepository repository, string branchName)
        {
            Guard.ArgumentNotNull(repository, nameof(repository));
            Guard.ArgumentNotEmptyString(branchName, nameof(branchName));

            return Task.Run(() =>
            {
                if (repository is Repository repo)
                {
                    LibGit2Sharp.Commands.Checkout(repo, branchName);
                }
                else
                {
                    log.Error("Couldn't checkout because {Variable} isn't an instance of {Type}", nameof(repository), typeof(Repository));
                }
            });
        }

        public async Task<bool> CommitExists(IRepository repository, string sha)
        {
            return await Task.Run(() => repository.Lookup<Commit>(sha) != null).ConfigureAwait(false);
        }

        public Task CreateBranch(IRepository repository, string branchName)
        {
            Guard.ArgumentNotNull(repository, nameof(repository));
            Guard.ArgumentNotEmptyString(branchName, nameof(branchName));

            return Task.Run(() =>
            {
                repository.CreateBranch(branchName);
            });
        }

        public Task<T> GetConfig<T>(IRepository repository, string key)
        {
            Guard.ArgumentNotNull(repository, nameof(repository));
            Guard.ArgumentNotEmptyString(key, nameof(key));

            return Task.Run(() =>
            {
                var result = repository.Config.Get<T>(key);
                return result != null ? result.Value : default(T);
            });
        }

        public Task SetConfig(IRepository repository, string key, string value)
        {
            Guard.ArgumentNotNull(repository, nameof(repository));
            Guard.ArgumentNotEmptyString(key, nameof(key));
            Guard.ArgumentNotEmptyString(value, nameof(value));

            return Task.Run(() =>
            {
                repository.Config.Set(key, value);
            });
        }

        public Task SetRemote(IRepository repository, string remoteName, Uri url)
        {
            Guard.ArgumentNotNull(repository, nameof(repository));
            Guard.ArgumentNotEmptyString(remoteName, nameof(remoteName));

            return Task.Run(() =>
            {
                repository.Config.Set("remote." + remoteName + ".url", url.ToString());
                repository.Config.Set("remote." + remoteName + ".fetch", "+refs/heads/*:refs/remotes/" + remoteName + "/*");
            });
        }

        public Task SetTrackingBranch(IRepository repository, string branchName, string remoteName)
        {
            Guard.ArgumentNotNull(repository, nameof(repository));
            Guard.ArgumentNotEmptyString(branchName, nameof(branchName));
            Guard.ArgumentNotEmptyString(remoteName, nameof(remoteName));

            return Task.Run(() =>
            {
                var remoteBranchName = IsCanonical(remoteName) ? remoteName : "refs/remotes/" + remoteName + "/" + branchName;
                var remoteBranch = repository.Branches[remoteBranchName];
                // if it's null, it's because nothing was pushed
                if (remoteBranch != null)
                {
                    var localBranchName = IsCanonical(branchName) ? branchName : "refs/heads/" + branchName;
                    var localBranch = repository.Branches[localBranchName];
                    repository.Branches.Update(localBranch, b => b.TrackedBranch = remoteBranch.CanonicalName);
                }
            });
        }

        public Task UnsetConfig(IRepository repository, string key)
        {
            Guard.ArgumentNotEmptyString(key, nameof(key));

            return Task.Run(() =>
            {
                repository.Config.Unset(key);
            });
        }

        public Task<Remote> GetHttpRemote(IRepository repo, string remote)
        {
            Guard.ArgumentNotNull(repo, nameof(repo));
            Guard.ArgumentNotEmptyString(remote, nameof(remote));

            return Task.Run(() =>
            {
                var uri = gitService.GetRemoteUri(repo, remote);
                var remoteName = uri.IsHypertextTransferProtocol ? remote : remote + "-http";
                var ret = repo.Network.Remotes[remoteName];
                if (ret == null)
                    ret = repo.Network.Remotes.Add(remoteName, UriString.ToUriString(uri.ToRepositoryUrl()));
                return ret;
            });
        }

        public Task<string> ExtractFile(IRepository repository, string commitSha, string relativePath)
        {
            Guard.ArgumentNotNull(repository, nameof(repository));
            Guard.ArgumentNotEmptyString(commitSha, nameof(commitSha));
            Guard.ArgumentIsRelativePath(relativePath, nameof(relativePath));

            var gitPath = Paths.ToGitPath(relativePath);
            return Task.Run(() =>
            {
                var commit = repository.Lookup<Commit>(commitSha);
                if (commit == null)
                {
                    throw new FileNotFoundException("Couldn't find '" + gitPath + "' at commit " + commitSha + ".");
                }

                var blob = commit[gitPath]?.Target as Blob;
                return blob?.GetContentText();
            });
        }

        public Task<byte[]> ExtractFileBinary(IRepository repository, string commitSha, string relativePath)
        {
            Guard.ArgumentNotNull(repository, nameof(repository));
            Guard.ArgumentNotEmptyString(commitSha, nameof(commitSha));
            Guard.ArgumentIsRelativePath(relativePath, nameof(relativePath));

            var gitPath = Paths.ToGitPath(relativePath);
            return Task.Run(() =>
            {
                var commit = repository.Lookup<Commit>(commitSha);
                if (commit == null)
                {
                    throw new FileNotFoundException("Couldn't find '" + gitPath + "' at commit " + commitSha + ".");
                }

                var blob = commit[gitPath]?.Target as Blob;

                if (blob != null)
                {
                    using (var m = new MemoryStream())
                    {
                        var content = blob.GetContentStream();
                        content.CopyTo(m);
                        return m.ToArray();
                    }
                }

                return null;
            });
        }

        public Task<bool> IsModified(IRepository repository, string relativePath, byte[] contents)
        {
            Guard.ArgumentNotNull(repository, nameof(repository));
            Guard.ArgumentIsRelativePath(relativePath, nameof(relativePath));

            var gitPath = Paths.ToGitPath(relativePath);
            return Task.Run(() =>
            {
                if (repository.RetrieveStatus(gitPath) == FileStatus.Unaltered)
                {
                    var treeEntry = repository.Head[gitPath];
                    if (treeEntry?.TargetType != TreeEntryTargetType.Blob)
                    {
                        return false;
                    }

                    var blob1 = (Blob)treeEntry.Target;
                    using (var s = contents != null ? new MemoryStream(contents) : new MemoryStream())
                    {
                        var blob2 = repository.ObjectDatabase.CreateBlob(s, gitPath);
                        var diff = repository.Diff.Compare(blob1, blob2);
                        return diff.LinesAdded != 0 || diff.LinesDeleted != 0;
                    }
                }

                return true;
            });
        }

        public async Task<string> GetPullRequestMergeBase(IRepository repo,
            UriString targetCloneUrl, string baseSha, string headSha, string baseRef, int pullNumber)
        {
            Guard.ArgumentNotNull(repo, nameof(repo));
            Guard.ArgumentNotNull(targetCloneUrl, nameof(targetCloneUrl));
            Guard.ArgumentNotEmptyString(baseRef, nameof(baseRef));

            var headCommit = repo.Lookup<Commit>(headSha);
            if (headCommit == null)
            {
                // The PR base branch might no longer exist, so we fetch using `refs/pull/<PR>/head` first.
                // This will often fetch the base commits, even when the base branch no longer exists.
                var headRef = $"refs/pull/{pullNumber}/head";
                await Fetch(repo, targetCloneUrl, headRef);
                headCommit = repo.Lookup<Commit>(headSha);
                if (headCommit == null)
                {
                    throw new NotFoundException($"Couldn't find {headSha} after fetching from {targetCloneUrl}:{headRef}.");
                }
            }

            var baseCommit = repo.Lookup<Commit>(baseSha);
            if (baseCommit == null)
            {
                await Fetch(repo, targetCloneUrl, baseRef);
                baseCommit = repo.Lookup<Commit>(baseSha);
                if (baseCommit == null)
                {
                    throw new NotFoundException($"Couldn't find {baseSha} after fetching from {targetCloneUrl}:{baseRef}.");
                }
            }

            var mergeBaseCommit = repo.ObjectDatabase.FindMergeBase(baseCommit, headCommit);
            if (mergeBaseCommit == null)
            {
                throw new NotFoundException($"Couldn't find merge base between {baseCommit} and {headCommit}.");
            }

            return mergeBaseCommit.Sha;
        }

        public Task<bool> IsHeadPushed(IRepository repo)
        {
            Guard.ArgumentNotNull(repo, nameof(repo));

            return Task.Run(() =>
            {
                return repo.Head.TrackingDetails.AheadBy == 0;
            });
        }

        public Task<IReadOnlyList<CommitMessage>> GetMessagesForUniqueCommits(
            IRepository repo,
            string baseBranch,
            string compareBranch,
            int maxCommits)
        {
            return Task.Run(() =>
            {
                var baseCommit = repo.Lookup<Commit>(baseBranch);
                var compareCommit = repo.Lookup<Commit>(compareBranch);
                if (baseCommit == null || compareCommit == null)
                {
                    var missingBranch = baseCommit == null ? baseBranch : compareBranch;
                    throw new NotFoundException(missingBranch);
                }

                var mergeCommit = repo.ObjectDatabase.FindMergeBase(baseCommit, compareCommit);
                var commitFilter = new CommitFilter
                {
                    IncludeReachableFrom = baseCommit,
                    ExcludeReachableFrom = mergeCommit,
                };

                var commits = repo.Commits
                    .QueryBy(commitFilter)
                    .Take(maxCommits)
                    .Select(c => new CommitMessage(c.Message))
                    .ToList();

                return (IReadOnlyList<CommitMessage>)commits;
            });
        }

        static bool IsCanonical(string s)
        {
            Guard.ArgumentNotEmptyString(s, nameof(s));

            return s.StartsWith("refs/", StringComparison.Ordinal);
        }
    }
}
