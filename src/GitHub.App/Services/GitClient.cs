using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
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

        public IObservable<Unit> Push(IRepository repository, string branchName, string remoteName)
        {
            Guard.ArgumentNotEmptyString(branchName, nameof(branchName));
            Guard.ArgumentNotEmptyString(remoteName, nameof(remoteName));

            return Observable.Defer(() =>
            {
                if (repository.Head?.Commits != null && repository.Head.Commits.Any())
                {
                    var remote = repository.Network.Remotes[remoteName];
                    repository.Network.Push(remote, "HEAD", @"refs/heads/" + branchName, pushOptions);
                }
                return Observable.Return(Unit.Default);
            });
        }

        public IObservable<Unit> Fetch(IRepository repository, string remoteName)
        {
            Guard.ArgumentNotEmptyString(remoteName, nameof(remoteName));

            return Observable.Defer(() =>
            {
                var remote = repository.Network.Remotes[remoteName];
                repository.Network.Fetch(remote, fetchOptions);
                return Observable.Return(Unit.Default);
            });
        }

        public IObservable<Unit> Fetch(IRepository repository, string remoteName, params string[] refspecs)
        {
            Guard.ArgumentNotEmptyString(remoteName, nameof(remoteName));

            return Observable.Defer(() =>
            {
                var remote = repository.Network.Remotes[remoteName];
                repository.Network.Fetch(remote, refspecs, fetchOptions);
                return Observable.Return(Unit.Default);
            });
        }

        public IObservable<Unit> Checkout(IRepository repository, string branchName)
        {
            Guard.ArgumentNotEmptyString(branchName, nameof(branchName));

            return Observable.Defer(() =>
            {
                repository.Checkout(branchName);
                return Observable.Return(Unit.Default);
            });
        }

        public IObservable<Unit> Merge(IRepository repository, string refToMerge)
        {
            Guard.ArgumentNotEmptyString(refToMerge, nameof(refToMerge));

            return Observable.Defer(() =>
            {
                var signature = repository.Config.BuildSignature(DateTimeOffset.Now);
                repository.Merge(refToMerge, signature);
                return Observable.Return(Unit.Default);
            });
        }

        public IObservable<Unit> Pull(IRepository repository)
        {
            return Observable.Defer(() =>
            {
                var signature = repository.Config.BuildSignature(DateTimeOffset.Now);
                repository.Network.Pull(signature, new PullOptions());
                return Observable.Return(Unit.Default);
            });
        }

        public IObservable<Branch> GetBranchStartsWith(IRepository repository, string s)
        {
            return Observable.Defer(() =>
            {
                var result = new List<Branch>();

                foreach (var branch in repository.Branches)
                {
                    if (branch.FriendlyName.StartsWith(s, StringComparison.Ordinal))
                    {
                        result.Add(branch);
                    }
                }

                return result.ToObservable();
            });
        }

        public IObservable<Unit> SetRemote(IRepository repository, string remoteName, Uri url)
        {
            Guard.ArgumentNotEmptyString(remoteName, nameof(remoteName));

            return Observable.Defer(() =>
            {
                repository.Config.Set("remote." + remoteName + ".url", url.ToString());
                repository.Config.Set("remote." + remoteName + ".fetch", "+refs/heads/*:refs/remotes/" + remoteName + "/*");

                return Observable.Return(Unit.Default);
            });
        }

        public IObservable<Unit> EnsurePullRequestsFetchRefExists(IRepository repository)
        {
            return Observable.Defer(() =>
            {
                var origin = repository.Network.Remotes["origin"];
                repository.Config.Set("remote.vspullrequests.url", origin.Url);
                repository.Config.Set("remote.vspullrequests.fetch", "+refs/pull/*/head:refs/remotes/vspullrequests/pull/*");
                return Observable.Return(Unit.Default);
            });
        }

        public IObservable<Unit> SetTrackingBranch(IRepository repository, string branchName, string remoteName)
        {
            Guard.ArgumentNotEmptyString(branchName, nameof(branchName));
            Guard.ArgumentNotEmptyString(remoteName, nameof(remoteName));

            return Observable.Defer(() =>
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
                return Observable.Return(Unit.Default);
            });
        }

        public IObservable<Remote> GetHttpRemote(IRepository repo, string remote)
        {
            return Observable.Defer(() => Observable.Return(GitService.GitServiceHelper.GetRemoteUri(repo, remote)))
                .Select(uri => new { Remote = uri.IsHypertextTransferProtocol ? remote : remote + "-http", Uri = uri })
                .Select(r =>
                {
                    var ret = repo.Network.Remotes[r.Remote];
                    if (ret == null)
                        ret = repo.Network.Remotes.Add(r.Remote, UriString.ToUriString(r.Uri.ToRepositoryUrl()));
                    return ret;
                });
        }

        private static bool IsCanonical(string s)
        {
            return s.StartsWith("refs/", StringComparison.Ordinal);
        }
    }
}
