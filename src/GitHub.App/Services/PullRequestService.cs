using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitHub.Models;
using LibGit2Sharp;
using Rothko;

namespace GitHub.Services
{
    [NullGuard.NullGuard(NullGuard.ValidationFlags.None)]
    [Export(typeof(IPullRequestService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PullRequestService : IPullRequestService
    {
        static readonly Regex InvalidBranchCharsRegex = new Regex(@"[^0-9A-Za-z_'\-]", RegexOptions.ECMAScript);

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
            ISimpleRepositoryModel sourceRepository, ISimpleRepositoryModel targetRepository,
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

        public async Task Checkout(ISimpleRepositoryModel repository, IPullRequestModel pullRequest)
        {
            var repo = gitService.GetRepository(repository.LocalPath);
            var localBranch = await GetPullRequestBranchName(repo, pullRequest);
            var remoteBranch = $"refs/pull/{pullRequest.Number}/head";
            var refspec = $"{remoteBranch}:{localBranch}";
            await gitClient.Fetch(repo, "origin", refspec);
            await gitClient.Checkout(repo, localBranch);
        }

        public IObservable<string> GetPullRequestTemplate(ISimpleRepositoryModel repository)
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
        /// Given a repository and a pull request returns the name of a local branch.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequest">The pull request.</param>
        /// <returns>The local branch name.</returns>
        /// <remarks>
        /// This method first tries to find an existing tracking branch that tracks the pull request. If
        /// that is found it returns this branch's canonical name. If not, it generates a name based on the
        /// pull request name and number and returns the canonical name of the branch to create.
        /// </remarks>
        async Task<string> GetPullRequestBranchName(IRepository repository, IPullRequestModel pullRequest)
        {
            var branch = $"refs/pull/{pullRequest.Number}/head";
            var existing = await gitClient.GetTrackingBranch(repository, branch).DefaultIfEmpty();

            if (existing != null)
            {
                return existing.CanonicalName;
            }
            else
            {
                return $"refs/heads/pr/{pullRequest.Number}-{GetSafeBranchName(pullRequest.Title)}";
            }
        }

        async Task<IPullRequestModel> PushAndCreatePR(IRepositoryHost host,
            ISimpleRepositoryModel sourceRepository, ISimpleRepositoryModel targetRepository,
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
                .ToLower();
        }
    }
}