using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Primitives;
using LibGit2Sharp;
using NLog;
using Rothko;

namespace GitHub.Services
{
    [NullGuard.NullGuard(NullGuard.ValidationFlags.None)]
    [Export(typeof(IPullRequestService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PullRequestService : IPullRequestService
    {
        static readonly Regex InvalidBranchCharsRegex = new Regex(@"[^0-9A-Za-z_'\-]", RegexOptions.ECMAScript);
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        static readonly string[] TemplatePaths = new[]
        {
            "PULL_REQUEST_TEMPLATE.md",
            "PULL_REQUEST_TEMPLATE",
            ".github\\PULL_REQUEST_TEMPLATE.md",
            ".github\\PULL_REQUEST_TEMPLATE",
        };

        readonly IGitClient gitClient;
        readonly IGitService gitService;
        readonly IOperatingSystem os;
        readonly IUsageTracker usageTracker;

        [ImportingConstructor]
        public PullRequestService(IGitClient gitClient, IGitService gitService, IOperatingSystem os, IUsageTracker usageTracker)
        {
            this.gitClient = gitClient;
            this.gitService = gitService;
            this.os = os;
            this.usageTracker = usageTracker;
        }

        public IObservable<IPullRequestModel> CreatePullRequest(IRepositoryHost host,
            ILocalRepositoryModel sourceRepository, IRepositoryModel targetRepository,
            IBranch sourceBranch, IBranch targetBranch,
            string title, string body
        )
        {
            Extensions.Guard.ArgumentNotNull(host, nameof(host));
            Extensions.Guard.ArgumentNotNull(sourceRepository, nameof(sourceRepository));
            Extensions.Guard.ArgumentNotNull(targetRepository, nameof(targetRepository));
            Extensions.Guard.ArgumentNotNull(sourceBranch, nameof(sourceBranch));
            Extensions.Guard.ArgumentNotNull(targetBranch, nameof(targetBranch));
            Extensions.Guard.ArgumentNotNull(title, nameof(title));
            Extensions.Guard.ArgumentNotNull(body, nameof(body));

            return PushAndCreatePR(host, sourceRepository, targetRepository, sourceBranch, targetBranch, title, body).ToObservable();
        }

        public async Task Checkout(ILocalRepositoryModel repository, IPullRequestModel pullRequest)
        {
            // NOTE: This currently requires a fetch refspec to be manually added to .git/config:
            //
            //     fetch = +refs/pull/*/head:refs/remotes/origin/pr/*
            //
            // As described here: https://gist.github.com/piscisaureus/3342247
            // It seems that currently libgit2sharp can't be used to add additional fetch refspecs.
            var repo = gitService.GetRepository(repository.LocalPath);
            var prCloneUrl = new UriString(pullRequest.Head.Repository.CloneUrl);
            var prRef = $"refs/remotes/origin/pull/{pullRequest.Number}";

            if (pullRequest.Head == null || pullRequest.Base == null)
            {
                log.Warn("Could not check out PR: IPullRequestModel.Head or Base were null. " +
                    "Assuming that they came from an old version's cache so ignoring.");
                return;
            }

            await gitClient.Fetch(repo, "origin");

            if (prCloneUrl.ToRepositoryUrl() == repository.CloneUrl.ToRepositoryUrl())
            {
                // The PR comes from the repository, so just check out the branch.
                await gitClient.Checkout(repo, pullRequest.Head.Ref);
                await gitClient.Pull(repo);
            }
            else
            {
                // The PR comes from a fork.
                var localBranch = await GetPullRequestBranch(repo, pullRequest);

                if (localBranch.Existing == null)
                {
                    // There's no existing local branch. Use a refspec to fetch the PR and create a new local
                    // branch for us.
                    var refspec = $"{prRef}:{localBranch.NewBranchName}";
                    await gitClient.Fetch(repo, "origin", refspec);
                    await gitClient.Checkout(repo, localBranch.NewBranchName);
                }
                else
                {
                    // There's an existing local branch. This may have local changes that have been committed
                    // since the last time it was fetched, so merge the remote ref into this branch.
                    await gitClient.Checkout(repo, localBranch.Existing.CanonicalName);
                    await gitClient.Merge(repo, prRef);
                }
            }
        }

        public IObservable<string> GetPullRequestTemplate(ILocalRepositoryModel repository)
        {
            Extensions.Guard.ArgumentNotNull(repository, nameof(repository));

            return Observable.Defer(() =>
            {
                var paths = TemplatePaths.Select(x => Path.Combine(repository.LocalPath, x));

                foreach (var path in paths)
                {
                    if (os.File.Exists(path))
                    {
                        try { return Observable.Return(os.File.ReadAllText(path, Encoding.UTF8)); } catch { }
                    }
                }
                return Observable.Empty<string>();
            });
        }

        /// <summary>
        /// Given a repository and a pull request returns either an existing local branch or the name of
        /// a branch to create.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequest">The pull request.</param>
        /// <returns>The canonical name of the local branch.</returns>
        /// <remarks>
        /// This method tries to find a branch whose name begins with "pr/{number}". If no branch is found it
        /// generates a branch name based on the pull request name and number.
        /// </remarks>
        async Task<NewOrExistingBranch> GetPullRequestBranch(IRepository repository, IPullRequestModel pullRequest)
        {
            var branch = $"refs/pull/{pullRequest.Number}/head";
            var existing = await gitClient.GetBranchStartsWith(repository, "pr/" + pullRequest.Number).DefaultIfEmpty();

            if (existing != null)
            {
                return new NewOrExistingBranch(existing);
            }
            else
            {
                var branchName = $"refs/heads/pr/{pullRequest.Number}-{GetSafeBranchName(pullRequest.Title)}";
                return new NewOrExistingBranch(branchName);
            }
        }

        async Task<IPullRequestModel> PushAndCreatePR(IRepositoryHost host,
            ILocalRepositoryModel sourceRepository, IRepositoryModel targetRepository,
            IBranch sourceBranch, IBranch targetBranch,
            string title, string body)
        {
            var repo = await Task.Run(() => gitService.GetRepository(sourceRepository.LocalPath));
            var remote = await gitClient.GetHttpRemote(repo, "origin");
            await gitClient.Push(repo, sourceBranch.Name, remote.Name);

            if (!repo.Branches[sourceBranch.Name].IsTracking)
                await gitClient.SetTrackingBranch(repo, sourceBranch.Name, remote.Name);

            // delay things a bit to avoid a race between pushing a new branch and creating a PR on it
            if (!Splat.ModeDetector.Current.InUnitTestRunner().GetValueOrDefault())
                await Task.Delay(TimeSpan.FromSeconds(5));

            var ret = await host.ModelService.CreatePullRequest(sourceRepository, targetRepository, sourceBranch, targetBranch, title, body);
            usageTracker.IncrementUpstreamPullRequestCount();
            return ret;
        }

        /// <summary>
        /// Given a repository name, returns a safe version with invalid characters replaced with dashes.
        /// </summary>
        static string GetSafeBranchName(string name)
        {
            return InvalidBranchCharsRegex
                .Replace(name, "-")
                .Replace("--", "-")
                .Replace("'", "")
                .ToLower(CultureInfo.CurrentCulture);
        }

        class NewOrExistingBranch
        {
            public NewOrExistingBranch(Branch existing)
            {
                Existing = existing;
            }

            public NewOrExistingBranch(string newBranchName)
            {
                NewBranchName = newBranchName;
            }

            public Branch Existing { get; }
            public string NewBranchName { get; }
        }
    }
}