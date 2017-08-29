using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using GitHub.Models;
using System.Reactive.Linq;
using Rothko;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Threading.Tasks;
using GitHub.Primitives;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Reactive;
using System.Collections.Generic;
using LibGit2Sharp;
using System.Diagnostics;

namespace GitHub.Services
{
    [Export(typeof(IPullRequestService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PullRequestService : IPullRequestService
    {
        const string SettingCreatedByGHfVS = "created-by-ghfvs";
        const string SettingGHfVSPullRequest = "ghfvs-pr-owner-number";

        static readonly Regex InvalidBranchCharsRegex = new Regex(@"[^0-9A-Za-z\-]", RegexOptions.ECMAScript);
        static readonly Regex BranchCapture = new Regex(@"branch\.(?<branch>.+)\.ghfvs-pr", RegexOptions.ECMAScript);

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

        public IObservable<bool> IsWorkingDirectoryClean(ILocalRepositoryModel repository)
        {
            var repo = gitService.GetRepository(repository.LocalPath);
            return Observable.Return(!repo.RetrieveStatus().IsDirty);
        }

        public IObservable<Unit> Pull(ILocalRepositoryModel repository)
        {
            return Observable.Defer(() =>
            {
                var repo = gitService.GetRepository(repository.LocalPath);
                return gitClient.Pull(repo).ToObservable();
            });
        }

        public IObservable<Unit> Push(ILocalRepositoryModel repository)
        {
            return Observable.Defer(async () =>
            {
                var repo = gitService.GetRepository(repository.LocalPath);
                var remote = await gitClient.GetHttpRemote(repo, repo.Head.Remote.Name);
                return gitClient.Push(repo, repo.Head.TrackedBranch.UpstreamBranchCanonicalName, remote.Name).ToObservable();
            });
        }

        public IObservable<Unit> Checkout(ILocalRepositoryModel repository, IPullRequestModel pullRequest, string localBranchName)
        {
            return Observable.Defer(async () =>
            {
                var repo = gitService.GetRepository(repository.LocalPath);
                var existing = repo.Branches[localBranchName];

                if (existing != null)
                {
                    await gitClient.Checkout(repo, localBranchName);
                }
                else if (repository.CloneUrl.ToRepositoryUrl() == pullRequest.Head.RepositoryCloneUrl.ToRepositoryUrl())
                {
                    var remote = await gitClient.GetHttpRemote(repo, "origin");
                    await gitClient.Fetch(repo, remote.Name);
                    await gitClient.Checkout(repo, localBranchName);
                }
                else
                {
                    var refSpec = $"{pullRequest.Head.Ref}:{localBranchName}";
                    var remoteName = await CreateRemote(repo, pullRequest.Head.RepositoryCloneUrl);

                    await gitClient.Fetch(repo, remoteName);
                    await gitClient.Fetch(repo, remoteName, new[] { refSpec });
                    await gitClient.Checkout(repo, localBranchName);
                    await gitClient.SetTrackingBranch(repo, localBranchName, $"refs/remotes/{remoteName}/{pullRequest.Head.Ref}");
                }

                // Store the PR number in the branch config with the key "ghfvs-pr".
                var prConfigKey = $"branch.{localBranchName}.{SettingGHfVSPullRequest}";
                await gitClient.SetConfig(repo, prConfigKey, BuildGHfVSConfigKeyValue(pullRequest));

                return Observable.Return(Unit.Default);
            });
        }

        public IObservable<string> GetDefaultLocalBranchName(ILocalRepositoryModel repository, int pullRequestNumber, string pullRequestTitle)
        {
            return Observable.Defer(() =>
            {
                var initial = "pr/" + pullRequestNumber + "-" + GetSafeBranchName(pullRequestTitle);
                var current = initial;
                var repo = gitService.GetRepository(repository.LocalPath);
                var index = 2;

                while (repo.Branches[current] != null)
                {
                    current = initial + '-' + index++;
                }

                return Observable.Return(current.TrimEnd('-'));
            });
        }

        public IObservable<BranchTrackingDetails> CalculateHistoryDivergence(ILocalRepositoryModel repository, int pullRequestNumber)
        {
            return Observable.Defer(async () =>
            {
                var repo = gitService.GetRepository(repository.LocalPath);

                if (repo.Head.Remote != null)
                {
                    var remote = await gitClient.GetHttpRemote(repo, repo.Head.Remote.Name);
                    await gitClient.Fetch(repo, remote.Name);
                }

                return Observable.Return(repo.Head.TrackingDetails);
            });
        }

        public IObservable<TreeChanges> GetTreeChanges(ILocalRepositoryModel repository, IPullRequestModel pullRequest)
        {
            return Observable.Defer(async () =>
            {
                var repo = gitService.GetRepository(repository.LocalPath);
                var remote = await gitClient.GetHttpRemote(repo, "origin");
                await gitClient.Fetch(repo, remote.Name);
                var changes = await gitClient.Compare(repo, pullRequest.Base.Sha, pullRequest.Head.Sha, detectRenames: true);
                return Observable.Return(changes);
            });
        }

        public IObservable<IBranch> GetLocalBranches(ILocalRepositoryModel repository, IPullRequestModel pullRequest)
        {
            return Observable.Defer(() =>
            {
                var repo = gitService.GetRepository(repository.LocalPath);
                var result = GetLocalBranchesInternal(repository, repo, pullRequest).Select(x => new BranchModel(x, repository));
                return result.ToObservable();
            });
        }

        public IObservable<bool> EnsureLocalBranchesAreMarkedAsPullRequests(ILocalRepositoryModel repository, IPullRequestModel pullRequest)
        {
            return Observable.Defer(async () =>
            {
                var repo = gitService.GetRepository(repository.LocalPath);
                var branches = GetLocalBranchesInternal(repository, repo, pullRequest).Select(x => new BranchModel(x, repository));
                var result = false;

                foreach (var branch in branches)
                {
                    if (!await IsBranchMarkedAsPullRequest(repo, branch.Name, pullRequest))
                    {
                        await MarkBranchAsPullRequest(repo, branch.Name, pullRequest);
                        result = true;
                    }
                }

                return Observable.Return(result);
            });
        }

        public bool IsPullRequestFromRepository(ILocalRepositoryModel repository, IPullRequestModel pullRequest)
        {
            if (pullRequest.Head?.RepositoryCloneUrl != null)
            {
                return repository.CloneUrl.ToRepositoryUrl() == pullRequest.Head.RepositoryCloneUrl.ToRepositoryUrl();
            }

            return false;
        }

        public IObservable<Unit> SwitchToBranch(ILocalRepositoryModel repository, IPullRequestModel pullRequest)
        {
            return Observable.Defer(async () =>
            {
                var repo = gitService.GetRepository(repository.LocalPath);
                var branchName = GetLocalBranchesInternal(repository, repo, pullRequest).FirstOrDefault();

                Debug.Assert(branchName != null, "PullRequestService.SwitchToBranch called but no local branch found.");

                if (branchName != null)
                {
                    var remote = await gitClient.GetHttpRemote(repo, "origin");
                    await gitClient.Fetch(repo, remote.Name);

                    var branch = repo.Branches[branchName];

                    if (branch == null)
                    {
                        var trackedBranchName = $"refs/remotes/{remote.Name}/" + branchName;
                        var trackedBranch = repo.Branches[trackedBranchName];

                        if (trackedBranch != null)
                        {
                            branch = repo.CreateBranch(branchName, trackedBranch.Tip);
                            await gitClient.SetTrackingBranch(repo, branchName, trackedBranchName);
                        }
                        else
                        {
                            throw new InvalidOperationException($"Could not find branch '{trackedBranchName}'.");
                        }
                    }

                    await gitClient.Checkout(repo, branchName);
                    await MarkBranchAsPullRequest(repo, branchName, pullRequest);
                }

                return Observable.Return(Unit.Default);
            });
        }

        public IObservable<Tuple<string, int>> GetPullRequestForCurrentBranch(ILocalRepositoryModel repository)
        {
            return Observable.Defer(async () =>
            {
                var repo = gitService.GetRepository(repository.LocalPath);
                var configKey = string.Format(
                    CultureInfo.InvariantCulture,
                    "branch.{0}.{1}",
                    repo.Head.FriendlyName,
                    SettingGHfVSPullRequest);
                var value = await gitClient.GetConfig<string>(repo, configKey);
                return Observable.Return(ParseGHfVSConfigKeyValue(value));
            });
        }

        public IObservable<string> ExtractFile(
            ILocalRepositoryModel repository,
            IPullRequestModel pullRequest,
            string fileName,
            bool head,
            Encoding encoding)
        {
            return Observable.Defer(async () =>
            {
                var repo = gitService.GetRepository(repository.LocalPath);
                var remote = await gitClient.GetHttpRemote(repo, "origin");
                string sha;

                if (head)
                {
                    sha = pullRequest.Head.Sha;
                }
                else
                {
                    try
                    {
                        sha = await gitClient.GetPullRequestMergeBase(
                            repo,
                            pullRequest.Base.RepositoryCloneUrl,
                            pullRequest.Head.RepositoryCloneUrl,
                            pullRequest.Base.Sha,
                            pullRequest.Head.Sha,
                            pullRequest.Base.Ref,
                            pullRequest.Head.Ref);
                    }
                    catch (NotFoundException ex)
                    {
                        throw new NotFoundException($"The Pull Request file failed to load. Please check your network connection and click refresh to try again. If this issue persists, please let us know at support@github.com", ex);
                    }
                }

                var file = await ExtractToTempFile(repo, pullRequest.Number, sha, fileName, encoding);
                return Observable.Return(file);
            });
        }

        public Encoding GetEncoding(ILocalRepositoryModel repository, string relativePath)
        {
            var fullPath = Path.Combine(repository.LocalPath, relativePath);

            if (File.Exists(fullPath))
            {
                var encoding = Encoding.UTF8;
                if (HasPreamble(fullPath, encoding))
                {
                    return encoding;
                }
            }

            return Encoding.Default;
        }

        static bool HasPreamble(string file, Encoding encoding)
        {
            using (var stream = File.OpenRead(file))
            {
                foreach (var b in encoding.GetPreamble())
                {
                    if (b != stream.ReadByte())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public IObservable<Unit> RemoveUnusedRemotes(ILocalRepositoryModel repository)
        {
            return Observable.Defer(async () =>
            {
                var repo = gitService.GetRepository(repository.LocalPath);
                var usedRemotes = new HashSet<string>(
                    repo.Branches
                        .Where(x => !x.IsRemote && x.Remote != null)
                        .Select(x => x.Remote?.Name));

                foreach (var remote in repo.Network.Remotes)
                {
                    var key = $"remote.{remote.Name}.{SettingCreatedByGHfVS}";
                    var createdByUs = await gitClient.GetConfig<bool>(repo, key);

                    if (createdByUs && !usedRemotes.Contains(remote.Name))
                    {
                        repo.Network.Remotes.Remove(remote.Name);
                    }
                }

                return Observable.Return(Unit.Default);
            });
        }

        async Task<string> CreateRemote(IRepository repo, UriString cloneUri)
        {
            foreach (var remote in repo.Network.Remotes)
            {
                if (remote.Url == cloneUri)
                {
                    return remote.Name;
                }
            }

            var remoteName = CreateUniqueRemoteName(repo, cloneUri.Owner);
            await gitClient.SetRemote(repo, remoteName, new Uri(cloneUri));
            await gitClient.SetConfig(repo, $"remote.{remoteName}.{SettingCreatedByGHfVS}", "true");
            return remoteName;
        }

        string CreateUniqueRemoteName(IRepository repo, string name)
        {
            var uniqueName = name;
            var number = 1;

            while (repo.Network.Remotes[uniqueName] != null)
            {
                uniqueName = name + number++;
            }

            return uniqueName;
        }

        async Task<string> ExtractToTempFile(
            IRepository repo,
            int pullRequestNumber,
            string commitSha,
            string fileName,
            Encoding encoding)
        {
            string contents;

            try
            {
                contents = await gitClient.ExtractFile(repo, commitSha, fileName) ?? string.Empty;
            }
            catch (FileNotFoundException)
            {
                var pullHeadRef = $"refs/pull/{pullRequestNumber}/head";
                var remote = await gitClient.GetHttpRemote(repo, "origin");
                await gitClient.Fetch(repo, remote.Name, commitSha, pullHeadRef);
                contents = await gitClient.ExtractFile(repo, commitSha, fileName) ?? string.Empty;
            }

            return CreateTempFile(fileName, commitSha, contents, encoding);
        }

        static string CreateTempFile(string fileName, string commitSha, string contents, Encoding encoding)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempFileName = $"{Path.GetFileNameWithoutExtension(fileName)}@{commitSha}{Path.GetExtension(fileName)}";
            var tempFile = Path.Combine(tempDir, tempFileName);

            Directory.CreateDirectory(tempDir);
            File.WriteAllText(tempFile, contents, encoding);
            return tempFile;
        }

        IEnumerable<string> GetLocalBranchesInternal(
            ILocalRepositoryModel localRepository,
            IRepository repository,
            IPullRequestModel pullRequest)
        {
            if (IsPullRequestFromRepository(localRepository, pullRequest))
            {
                return new[] { pullRequest.Head.Ref };
            }
            else
            {
                var key = BuildGHfVSConfigKeyValue(pullRequest);

                return repository.Config
                    .Select(x => new { Branch = BranchCapture.Match(x.Key).Groups["branch"].Value, Value = x.Value })
                    .Where(x => !string.IsNullOrWhiteSpace(x.Branch) && x.Value == key)
                    .Select(x => x.Branch);
            }
        }

        async Task<bool> IsBranchMarkedAsPullRequest(IRepository repo, string branchName, IPullRequestModel pullRequest)
        {
            var prConfigKey = $"branch.{branchName}.{SettingGHfVSPullRequest}";
            var value = ParseGHfVSConfigKeyValue(await gitClient.GetConfig<string>(repo, prConfigKey));
            return value != null &&
                value.Item1 == pullRequest.Base.RepositoryCloneUrl.Owner &&
                value.Item2 == pullRequest.Number;
        }

        async Task MarkBranchAsPullRequest(IRepository repo, string branchName, IPullRequestModel pullRequest)
        {
            // Store the PR number in the branch config with the key "ghfvs-pr".
            var prConfigKey = $"branch.{branchName}.{SettingGHfVSPullRequest}";
            await gitClient.SetConfig(repo, prConfigKey, BuildGHfVSConfigKeyValue(pullRequest));
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
            await usageTracker.IncrementUpstreamPullRequestCount();
            return ret;
        }

        static string GetSafeBranchName(string name)
        {
            var before = InvalidBranchCharsRegex.Replace(name, "-").TrimEnd('-');

            for (;;)
            {
                string after = before.Replace("--", "-");

                if (after == before)
                {
                    return before.ToLower(CultureInfo.CurrentCulture);
                }

                before = after;
            }
        }

        static string BuildGHfVSConfigKeyValue(IPullRequestModel pullRequest)
        {
            return pullRequest.Base.RepositoryCloneUrl.Owner + '#' +
                   pullRequest.Number.ToString(CultureInfo.InvariantCulture);
        }

        static Tuple<string, int> ParseGHfVSConfigKeyValue(string value)
        {
            if (value != null)
            {
                var separator = value.IndexOf('#');

                if (separator != -1)
                {
                    var owner = value.Substring(0, separator);
                    int number;

                    if (int.TryParse(value.Substring(separator + 1), NumberStyles.None, CultureInfo.InvariantCulture, out number))
                    {
                        return Tuple.Create(owner, number);
                    }
                }
            }

            return null;
        }
    }
}
