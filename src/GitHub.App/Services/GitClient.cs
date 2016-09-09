using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using LibGit2Sharp;
using GitHub.Primitives;
using System.Collections.Generic;

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

        public IObservable<Branch> GetTrackingBranch(IRepository repository, string canonicalBranchName)
        {
            Extensions.Guard.ArgumentNotNull(repository, nameof(repository));
            Guard.ArgumentNotEmptyString(canonicalBranchName, nameof(canonicalBranchName));

            // TODO: Does this need to be run async?
            return Observable.Defer(() =>
            {
                // TODO: Assuming remote is called "origin" here. This may not be the case.
                // TODO: Locally checked out branch tracking the PR might not be marked as a tracking branch.
                //       We know were the PR is coming from, so can set up the remote, fetch, and then scan 
                //       the local branches that aren’t tracking remote branches and check if any of their
                //       heads match the PR head. This is where we probably would want to ask the user if 
                //       they want to use that branch for working on the PR, and if yes, we set it to properly
                //       track the remote branch for fetching
                var remote = repository.Network.Remotes["origin"];
                var result = new List<Branch>();

                // Get the ref from the remote that matches 'branchName'.
                var remoteRef = repository.Network.ListReferences(remote)
                    .Where(x => x.CanonicalName == canonicalBranchName)
                    .SingleOrDefault();

                if (remoteRef != null)
                {
                    foreach (var branch in repository.Branches)
                    {
                        if (!branch.IsRemote && branch.IsTracking && branch.TrackedBranch.Tip != null)
                        {
                            if (branch.TrackedBranch.Tip.Sha == remoteRef.TargetIdentifier)
                            {
                                result.Add(branch);
                            }
                        }
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
            return s.StartsWith("refs/");
        }
    }
}
