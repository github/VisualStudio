using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Primitives;
using LibGit2Sharp;

namespace GitHub.Services
{
    [Export(typeof(IGitClient))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GitClient : IGitClient
    {
        readonly PushOptions pushOptions;
        readonly FetchOptions fetchOptions;

        [ImportingConstructor]
        public GitClient(IGitHubCredentialProvider credentialProvider)
        {
            pushOptions = new PushOptions { CredentialsProvider = credentialProvider.HandleCredentials };
            fetchOptions = new FetchOptions { CredentialsProvider = credentialProvider.HandleCredentials };
        }

        public async Task Push(IRepository repository, string branchName, string remoteName)
        {
            Guard.ArgumentNotEmptyString(branchName, nameof(branchName));
            Guard.ArgumentNotEmptyString(remoteName, nameof(remoteName));

            var remote = await GetHttpRemote(repository, remoteName).ConfigureAwait(false);
            if (repository.Head?.Commits != null && repository.Head.Commits.Any())
            {
                repository.Network.Push(remote, "HEAD", @"refs/heads/" + branchName, pushOptions);
            }
        }

        public async Task Fetch(IRepository repository, string remoteName)
        {
            Guard.ArgumentNotEmptyString(remoteName, nameof(remoteName));

            var remote = await GetHttpRemote(repository, remoteName).ConfigureAwait(false);
            repository.Network.Fetch(remote, fetchOptions);
        }

        public async Task Fetch(IRepository repository, string remoteName, params string[] refspecs)
        {
            Guard.ArgumentNotEmptyString(remoteName, nameof(remoteName));

            var remote = await GetHttpRemote(repository, remoteName).ConfigureAwait(false);
            repository.Network.Fetch(remote, refspecs, fetchOptions);
        }

        public async Task Checkout(IRepository repository, string branchName)
        {
            Guard.ArgumentNotEmptyString(branchName, nameof(branchName));

            await TaskScheduler.Default;
            repository.Checkout(branchName);
        }

        public async Task SetConfig(IRepository repository, string key, string value)
        {
            Guard.ArgumentNotEmptyString(key, nameof(key));
            Guard.ArgumentNotEmptyString(value, nameof(value));

            await TaskScheduler.Default;
            repository.Config.Set(key, value);
        }

        public async Task SetRemote(IRepository repository, string remoteName, Uri url)
        {
            Guard.ArgumentNotEmptyString(remoteName, nameof(remoteName));

            await TaskScheduler.Default;

            repository.Config.Set("remote." + remoteName + ".url", url.ToString());
            repository.Config.Set("remote." + remoteName + ".fetch", "+refs/heads/*:refs/remotes/" + remoteName + "/*");
        }

        public async Task SetTrackingBranch(IRepository repository, string branchName, string remoteName)
        {
            Guard.ArgumentNotEmptyString(branchName, nameof(branchName));
            Guard.ArgumentNotEmptyString(remoteName, nameof(remoteName));

            await TaskScheduler.Default;

            var remoteBranchName = IsCanonical(remoteName) ? remoteName : "refs/remotes/" + remoteName + "/" + branchName;
            var remoteBranch = repository.Branches[remoteBranchName];
            // if it's null, it's because nothing was pushed
            if (remoteBranch != null)
            {
                var localBranchName = IsCanonical(branchName) ? branchName : "refs/heads/" + branchName;
                var localBranch = repository.Branches[localBranchName];
                repository.Branches.Update(localBranch, b => b.TrackedBranch = remoteBranch.CanonicalName);
            }
        }

        public async Task UnsetConfig(IRepository repository, string key)
        {
            Guard.ArgumentNotEmptyString(key, nameof(key));

            await TaskScheduler.Default;
            repository.Config.Unset(key);
        }

        public async Task<Remote> GetHttpRemote(IRepository repo, string remote)
        {
            await TaskScheduler.Default;

            var uri = GitService.GitServiceHelper.GetRemoteUri(repo, remote);
            var remoteName = uri.IsHypertextTransferProtocol ? remote : remote + "-http";
            var ret = repo.Network.Remotes[remoteName];
            if (ret == null)
                ret = repo.Network.Remotes.Add(remoteName, UriString.ToUriString(uri.ToRepositoryUrl()));
            return ret;
        }

        public async Task<string> ExtractFile(IRepository repository, string commitSha, string fileName)
        {
            var commit = repository.Lookup<Commit>(commitSha);
            var blob = commit[fileName]?.Target as Blob;

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempFileName = $"{Path.GetFileNameWithoutExtension(fileName)}@{commitSha}{Path.GetExtension(fileName)}";
            var tempFile = Path.Combine(tempDir, tempFileName);

            Directory.CreateDirectory(tempDir);

            if (blob != null)
            {
                using (var source = blob.GetContentStream(new FilteringOptions(fileName)))
                using (var destination = File.OpenWrite(tempFile))
                {
                    await source.CopyToAsync(destination);
                }
            }
            else
            {
                File.Create(tempFile).Dispose();
            }

            return tempFile;
        }

        static bool IsCanonical(string s)
        {
            return s.StartsWith("refs/", StringComparison.Ordinal);
        }
    }
}
