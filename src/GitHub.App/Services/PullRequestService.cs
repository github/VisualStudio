using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitHub.Api;
using GitHub.App.Services;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using LibGit2Sharp;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using Rothko;
using static System.FormattableString;
using static Octokit.GraphQL.Variable;
using CheckConclusionState = GitHub.Models.CheckConclusionState;
using CheckStatusState = GitHub.Models.CheckStatusState;
using StatusState = GitHub.Models.StatusState;

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
        static ICompiledQuery<Page<ActorModel>> readAssignableUsers;
        static ICompiledQuery<Page<PullRequestListItemModel>> readPullRequests;
        static ICompiledQuery<Page<PullRequestListItemModel>> readPullRequestsEnterprise;

        static readonly string[] TemplatePaths = new[]
        {
            "PULL_REQUEST_TEMPLATE.md",
            "PULL_REQUEST_TEMPLATE",
            ".github\\PULL_REQUEST_TEMPLATE.md",
            ".github\\PULL_REQUEST_TEMPLATE",
        };

        readonly IGitClient gitClient;
        readonly IGitService gitService;
        readonly IVSGitExt gitExt;
        readonly IGraphQLClientFactory graphqlFactory;
        readonly IOperatingSystem os;
        readonly IUsageTracker usageTracker;

        [ImportingConstructor]
        public PullRequestService(
            IGitClient gitClient,
            IGitService gitService,
            IVSGitExt gitExt,
            IGraphQLClientFactory graphqlFactory,
            IOperatingSystem os,
            IUsageTracker usageTracker)
        {
            this.gitClient = gitClient;
            this.gitService = gitService;
            this.gitExt = gitExt;
            this.graphqlFactory = graphqlFactory;
            this.os = os;
            this.usageTracker = usageTracker;
        }

        public async Task<Page<PullRequestListItemModel>> ReadPullRequests(
            HostAddress address,
            string owner,
            string name,
            string after,
            PullRequestStateEnum[] states)
        {

            ICompiledQuery<Page<PullRequestListItemModel>> query;

            if (address.IsGitHubDotCom())
            {
                if (readPullRequests == null)
                {
                    readPullRequests = new Query()
                          .Repository(Var(nameof(owner)), Var(nameof(name)))
                          .PullRequests(
                              first: 100,
                              after: Var(nameof(after)),
                              orderBy: new IssueOrder { Direction = OrderDirection.Desc, Field = IssueOrderField.CreatedAt },
                              states: Var(nameof(states)))
                          .Select(page => new Page<PullRequestListItemModel>
                          {
                              EndCursor = page.PageInfo.EndCursor,
                              HasNextPage = page.PageInfo.HasNextPage,
                              TotalCount = page.TotalCount,
                              Items = page.Nodes.Select(pr => new ListItemAdapter
                              {
                                  Id = pr.Id.Value,
                                  LastCommit = pr.Commits(null, null, 1, null).Nodes.Select(commit =>
                                      new LastCommitSummaryAdapter
                                      {
                                          CheckSuites = commit.Commit.CheckSuites(null, null, null, null, null).AllPages(10)
                                              .Select(suite => new CheckSuiteSummaryModel
                                              {
                                                  CheckRuns = suite.CheckRuns(null, null, null, null, null).AllPages(10)
                                                      .Select(run => new CheckRunSummaryModel
                                                      {
                                                          Conclusion = run.Conclusion.FromGraphQl(),
                                                          Status = run.Status.FromGraphQl()
                                                      }).ToList(),
                                              }).ToList(),
                                          Statuses = commit.Commit.Status
                                                  .Select(context =>
                                                      context.Contexts.Select(statusContext => new StatusSummaryModel
                                                      {
                                                          State = statusContext.State.FromGraphQl(),
                                                      }).ToList()
                                                  ).SingleOrDefault()
                                      }).ToList().FirstOrDefault(),
                                  Author = new ActorModel
                                  {
                                      Login = pr.Author.Login,
                                      AvatarUrl = pr.Author.AvatarUrl(null),
                                  },
                                  CommentCount = pr.Comments(0, null, null, null).TotalCount,
                                  Number = pr.Number,
                                  Reviews = pr.Reviews(null, null, null, null, null, null).AllPages().Select(review => new ReviewAdapter
                                  {
                                      Body = review.Body,
                                      CommentCount = review.Comments(null, null, null, null).TotalCount,
                                  }).ToList(),
                                  State = pr.State.FromGraphQl(),
                                  Title = pr.Title,
                                  UpdatedAt = pr.UpdatedAt,
                              }).ToList(),
                          }).Compile();
                }

                query = readPullRequests;
            }
            else
            {
                if (readPullRequestsEnterprise == null)
                {
                    readPullRequestsEnterprise = new Query()
                          .Repository(Var(nameof(owner)), Var(nameof(name)))
                          .PullRequests(
                              first: 100,
                              after: Var(nameof(after)),
                              orderBy: new IssueOrder { Direction = OrderDirection.Desc, Field = IssueOrderField.CreatedAt },
                              states: Var(nameof(states)))
                          .Select(page => new Page<PullRequestListItemModel>
                          {
                              EndCursor = page.PageInfo.EndCursor,
                              HasNextPage = page.PageInfo.HasNextPage,
                              TotalCount = page.TotalCount,
                              Items = page.Nodes.Select(pr => new ListItemAdapter
                              {
                                  Id = pr.Id.Value,
                                  LastCommit = pr.Commits(null, null, 1, null).Nodes.Select(commit =>
                                      new LastCommitSummaryAdapter
                                      {
                                          Statuses = commit.Commit.Status
                                                  .Select(context =>
                                                      context.Contexts.Select(statusContext => new StatusSummaryModel
                                                      {
                                                          State = statusContext.State.FromGraphQl(),
                                                      }).ToList()
                                                  ).SingleOrDefault()
                                      }).ToList().FirstOrDefault(),
                                  Author = new ActorModel
                                  {
                                      Login = pr.Author.Login,
                                      AvatarUrl = pr.Author.AvatarUrl(null),
                                  },
                                  CommentCount = pr.Comments(0, null, null, null).TotalCount,
                                  Number = pr.Number,
                                  Reviews = pr.Reviews(null, null, null, null, null, null).AllPages().Select(review => new ReviewAdapter
                                  {
                                      Body = review.Body,
                                      CommentCount = review.Comments(null, null, null, null).TotalCount,
                                  }).ToList(),
                                  State = pr.State.FromGraphQl(),
                                  Title = pr.Title,
                                  UpdatedAt = pr.UpdatedAt,
                              }).ToList(),
                          }).Compile();
                }

                query = readPullRequestsEnterprise;
            }

            var graphql = await graphqlFactory.CreateConnection(address);
            var vars = new Dictionary<string, object>
            {
                { nameof(owner), owner },
                { nameof(name), name },
                { nameof(after), after },
                { nameof(states), states.Select(x => (PullRequestState)x).ToList() },
            };

            var result = await graphql.Run(query, vars);

            foreach (var item in result.Items.Cast<ListItemAdapter>())
            {
                item.CommentCount += item.Reviews.Sum(x => x.Count);
                item.Reviews = null;

                var checkRuns = item.LastCommit?.CheckSuites?.SelectMany(model => model.CheckRuns).ToArray();

                var hasCheckRuns = checkRuns?.Any() ?? false;
                var hasStatuses = item.LastCommit?.Statuses?.Any() ?? false;

                if (!hasCheckRuns && !hasStatuses)
                {
                    item.Checks = PullRequestChecksState.None;
                }
                else
                {
                    var checksHasFailure = false;
                    var checksHasCompleteSuccess = true;

                    if (hasCheckRuns)
                    {
                        checksHasFailure = checkRuns
                            .Any(model => model.Conclusion.HasValue
                                          && (model.Conclusion.Value == CheckConclusionState.Failure
                                              || model.Conclusion.Value == CheckConclusionState.ActionRequired));

                        if (!checksHasFailure)
                        {
                            checksHasCompleteSuccess = checkRuns
                                .All(model => model.Conclusion.HasValue
                                              && (model.Conclusion.Value == CheckConclusionState.Success
                                                  || model.Conclusion.Value == CheckConclusionState.Neutral));
                        }
                    }

                    var statusHasFailure = false;
                    var statusHasCompleteSuccess = true;

                    if (!checksHasFailure && hasStatuses)
                    {
                        statusHasFailure = item.LastCommit
                            .Statuses
                            .Any(status => status.State == StatusState.Failure
                                           || status.State == StatusState.Error);

                        if (!statusHasFailure)
                        {
                            statusHasCompleteSuccess =
                                item.LastCommit.Statuses.All(status => status.State == StatusState.Success);
                        }
                    }

                    if (checksHasFailure || statusHasFailure)
                    {
                        item.Checks = PullRequestChecksState.Failure;
                    }
                    else if (statusHasCompleteSuccess && checksHasCompleteSuccess)
                    {
                        item.Checks = PullRequestChecksState.Success;
                    }
                    else
                    {
                        item.Checks = PullRequestChecksState.Pending;
                    }
                }

                item.LastCommit = null;
            }

            return result;
        }

        public async Task<Page<ActorModel>> ReadAssignableUsers(
            HostAddress address,
            string owner,
            string name,
            string after)
        {
            if (readAssignableUsers == null)
            {
                readAssignableUsers = new Query()
                    .Repository(Var(nameof(owner)), Var(nameof(name)))
                    .AssignableUsers(first: 100, after: Var(nameof(after)))
                    .Select(connection => new Page<ActorModel>
                    {
                        EndCursor = connection.PageInfo.EndCursor,
                        HasNextPage = connection.PageInfo.HasNextPage,
                        TotalCount = connection.TotalCount,
                        Items = connection.Nodes.Select(user => new ActorModel
                        {
                            AvatarUrl = user.AvatarUrl(30),
                            Login = user.Login,
                        }).ToList(),
                    }).Compile();
            }

            var graphql = await graphqlFactory.CreateConnection(address);
            var vars = new Dictionary<string, object>
            {
                { nameof(owner), owner },
                { nameof(name), name },
                { nameof(after), after },
            };

            return await graphql.Run(readAssignableUsers, vars);
        }

        public IObservable<IPullRequestModel> CreatePullRequest(IModelService modelService,
            LocalRepositoryModel sourceRepository, RepositoryModel targetRepository,
            BranchModel sourceBranch, BranchModel targetBranch,
            string title, string body
        )
        {
            Extensions.Guard.ArgumentNotNull(modelService, nameof(modelService));
            Extensions.Guard.ArgumentNotNull(sourceRepository, nameof(sourceRepository));
            Extensions.Guard.ArgumentNotNull(targetRepository, nameof(targetRepository));
            Extensions.Guard.ArgumentNotNull(sourceBranch, nameof(sourceBranch));
            Extensions.Guard.ArgumentNotNull(targetBranch, nameof(targetBranch));
            Extensions.Guard.ArgumentNotNull(title, nameof(title));
            Extensions.Guard.ArgumentNotNull(body, nameof(body));

            return PushAndCreatePR(modelService, sourceRepository, targetRepository, sourceBranch, targetBranch, title, body).ToObservable();
        }

        public IObservable<string> GetPullRequestTemplate(LocalRepositoryModel repository)
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

        public IObservable<IReadOnlyList<CommitMessage>> GetMessagesForUniqueCommits(
            LocalRepositoryModel repository,
            string baseBranch,
            string compareBranch,
            int maxCommits)
        {
            return Observable.Defer(async () =>
            {
                // CommitMessage doesn't keep a reference to Repository
                using (var repo = gitService.GetRepository(repository.LocalPath))
                {
                    var messages = await gitClient.GetMessagesForUniqueCommits(repo, baseBranch, compareBranch, maxCommits);
                    return Observable.Return(messages);
                }
            });
        }

        public IObservable<int> CountSubmodulesToSync(LocalRepositoryModel repository)
        {
            using (var repo = gitService.GetRepository(repository.LocalPath))
            {
                var count = 0;
                foreach (var submodule in repo.Submodules)
                {
                    var status = submodule.RetrieveStatus();
                    if ((status & SubmoduleStatus.WorkDirAdded) != 0)
                    {
                        count++;
                    }
                    else if ((status & SubmoduleStatus.WorkDirDeleted) != 0)
                    {
                        count++;
                    }
                    else if ((status & SubmoduleStatus.WorkDirModified) != 0)
                    {
                        count++;
                    }
                    else if ((status & SubmoduleStatus.WorkDirUninitialized) != 0)
                    {
                        count++;
                    }
                }

                return Observable.Return(count);
            }
        }

        public IObservable<bool> IsWorkingDirectoryClean(LocalRepositoryModel repository)
        {
            // The `using` appears to resolve this issue:
            // https://github.com/github/VisualStudio/issues/1306
            using (var repo = gitService.GetRepository(repository.LocalPath))
            {
                var statusOptions = new StatusOptions { ExcludeSubmodules = true };
                var status = repo.RetrieveStatus(statusOptions);
                var isClean = !IsCheckoutBlockingDirty(status);
                return Observable.Return(isClean);
            }
        }

        static bool IsCheckoutBlockingDirty(RepositoryStatus status)
        {
            if (status.IsDirty)
            {
                return status.Any(entry => IsCheckoutBlockingChange(entry));
            }

            return false;
        }

        // This is similar to IsDirty, but also allows NewInWorkdir and DeletedFromWorkdir files
        static bool IsCheckoutBlockingChange(StatusEntry entry)
        {
            switch (entry.State)
            {
                case FileStatus.Ignored:
                    return false;
                case FileStatus.Unaltered:
                    return false;
                case FileStatus.NewInWorkdir:
                    return false;
                case FileStatus.DeletedFromWorkdir:
                    return false;
                default:
                    return true;
            }
        }

        public IObservable<Unit> Pull(LocalRepositoryModel repository)
        {
            return Observable.Defer(async () =>
            {
                using (var repo = gitService.GetRepository(repository.LocalPath))
                {
                    await gitClient.Pull(repo);
                    return Observable.Return(Unit.Default);
                }
            });
        }

        public IObservable<Unit> Push(LocalRepositoryModel repository)
        {
            return Observable.Defer(async () =>
            {
                using (var repo = gitService.GetRepository(repository.LocalPath))
                {
                    var remoteName = repo.Head.RemoteName;
                    var remote = await gitClient.GetHttpRemote(repo, remoteName);
                    await gitClient.Push(repo, repo.Head.TrackedBranch.UpstreamBranchCanonicalName, remote.Name);
                    return Observable.Return(Unit.Default);
                }
            });
        }

        public async Task<bool> SyncSubmodules(LocalRepositoryModel repository, Action<string> progress)
        {
            var exitCode = await Where("git");
            if (exitCode != 0)
            {
                progress(Resources.CouldntFindGitOnPath);
                return false;
            }

            return await SyncSubmodules(repository.LocalPath, progress) == 0;
        }

        // LibGit2Sharp has limited submodule support so shelling out Git.exe for submodule commands.
        async Task<int> SyncSubmodules(string workingDir, Action<string> progress)
        {
            var cmdArguments = "/C git submodule init & git submodule sync --recursive & git submodule update --recursive";
            var startInfo = new ProcessStartInfo("cmd", cmdArguments)
            {
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(startInfo))
            {
                await Task.WhenAll(
                    ReadLinesAsync(process.StandardOutput, progress),
                    ReadLinesAsync(process.StandardError, progress),
                    Task.Run(() => process.WaitForExit()));
                return process.ExitCode;
            }
        }

        static Task<int> Where(string fileName)
        {
            return Task.Run(() =>
            {
                var cmdArguments = "/C WHERE /Q " + fileName;
                var startInfo = new ProcessStartInfo("cmd", cmdArguments)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    return process.ExitCode;
                }
            });
        }

        static async Task ReadLinesAsync(TextReader reader, Action<string> progress)
        {
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                progress(line);
            }
        }

        public IObservable<Unit> Checkout(LocalRepositoryModel repository, PullRequestDetailModel pullRequest, string localBranchName)
        {
            return Observable.Defer(async () =>
            {
                using (var repo = gitService.GetRepository(repository.LocalPath))
                {
                    var existing = repo.Branches[localBranchName];

                    if (existing != null)
                    {
                        await gitClient.Checkout(repo, localBranchName);
                    }
                    else if (string.Equals(repository.CloneUrl.Owner, pullRequest.HeadRepositoryOwner, StringComparison.OrdinalIgnoreCase))
                    {
                        var remote = await gitClient.GetHttpRemote(repo, "origin");
                        await gitClient.Fetch(repo, remote.Name);
                        await gitClient.Checkout(repo, localBranchName);
                    }
                    else
                    {
                        var refSpec = $"{pullRequest.HeadRefName}:{localBranchName}";
                        var remoteName = await CreateRemote(repo, repository.CloneUrl.WithOwner(pullRequest.HeadRepositoryOwner));

                        await gitClient.Fetch(repo, remoteName);
                        await gitClient.Fetch(repo, remoteName, new[] { refSpec });
                        await gitClient.Checkout(repo, localBranchName);
                        await gitClient.SetTrackingBranch(repo, localBranchName, $"refs/remotes/{remoteName}/{pullRequest.HeadRefName}");
                    }

                    // Store the PR number in the branch config with the key "ghfvs-pr".
                    var prConfigKey = $"branch.{localBranchName}.{SettingGHfVSPullRequest}";
                    await gitClient.SetConfig(repo, prConfigKey, BuildGHfVSConfigKeyValue(pullRequest.BaseRepositoryOwner, pullRequest.Number));

                    return Observable.Return(Unit.Default);
                }
            });
        }

        public IObservable<string> GetDefaultLocalBranchName(LocalRepositoryModel repository, int pullRequestNumber, string pullRequestTitle)
        {
            return Observable.Defer(() =>
            {
                var initial = "pr/" + pullRequestNumber + "-" + GetSafeBranchName(pullRequestTitle);
                var current = initial;
                using (var repo = gitService.GetRepository(repository.LocalPath))
                {
                    var index = 2;

                    while (repo.Branches[current] != null)
                    {
                        current = initial + '-' + index++;
                    }
                }

                return Observable.Return(current.TrimEnd('-'));
            });
        }

        public IObservable<BranchTrackingDetails> CalculateHistoryDivergence(LocalRepositoryModel repository, int pullRequestNumber)
        {
            return Observable.Defer(async () =>
            {
                // BranchTrackingDetails doesn't keep a reference to Repository
                using (var repo = gitService.GetRepository(repository.LocalPath))
                {
                    var remoteName = repo.Head.RemoteName;
                    if (remoteName != null)
                    {
                        var remote = await gitClient.GetHttpRemote(repo, remoteName);
                        await gitClient.Fetch(repo, remote.Name);
                    }

                    return Observable.Return(repo.Head.TrackingDetails);
                }
            });
        }

        public async Task<string> GetMergeBase(LocalRepositoryModel repository, PullRequestDetailModel pullRequest)
        {
            using (var repo = gitService.GetRepository(repository.LocalPath))
            {
                return await gitClient.GetPullRequestMergeBase(
                    repo,
                    repository.CloneUrl.WithOwner(pullRequest.BaseRepositoryOwner),
                    pullRequest.BaseRefSha,
                    pullRequest.HeadRefSha,
                    pullRequest.BaseRefName,
                    pullRequest.Number);
            }
        }

        public IObservable<TreeChanges> GetTreeChanges(LocalRepositoryModel repository, PullRequestDetailModel pullRequest)
        {
            return Observable.Defer(async () =>
            {
                // TreeChanges doesn't keep a reference to Repository
                using (var repo = gitService.GetRepository(repository.LocalPath))
                {
                    var remote = await gitClient.GetHttpRemote(repo, "origin");
                    await gitClient.Fetch(repo, remote.Name);
                    var changes = await gitClient.Compare(repo, pullRequest.BaseRefSha, pullRequest.HeadRefSha, detectRenames: true);
                    return Observable.Return(changes);
                }
            });
        }

        public IObservable<BranchModel> GetLocalBranches(LocalRepositoryModel repository, PullRequestDetailModel pullRequest)
        {
            return Observable.Defer(() =>
            {
                // BranchModel doesn't keep a reference to rep
                using (var repo = gitService.GetRepository(repository.LocalPath))
                {
                    var result = GetLocalBranchesInternal(repository, repo, pullRequest).Select(x => new BranchModel(x, repository));
                    return result.ToList().ToObservable();
                }
            });
        }

        public IObservable<bool> EnsureLocalBranchesAreMarkedAsPullRequests(LocalRepositoryModel repository, PullRequestDetailModel pullRequest)
        {
            return Observable.Defer(async () =>
            {
                using (var repo = gitService.GetRepository(repository.LocalPath))
                {
                    var branches = GetLocalBranchesInternal(repository, repo, pullRequest).Select(x => new BranchModel(x, repository));
                    var result = false;

                    foreach (var branch in branches)
                    {
                        if (!await IsBranchMarkedAsPullRequest(repo, branch.Name, pullRequest))
                        {
                            await MarkBranchAsPullRequest(repo, branch.Name, pullRequest.BaseRepositoryOwner, pullRequest.Number);
                            result = true;
                        }
                    }

                    return Observable.Return(result);
                }
            });
        }

        public bool IsPullRequestFromRepository(LocalRepositoryModel repository, PullRequestDetailModel pullRequest)
        {
            return string.Equals(repository.CloneUrl?.Owner, pullRequest.HeadRepositoryOwner, StringComparison.OrdinalIgnoreCase);
        }

        public IObservable<Unit> SwitchToBranch(LocalRepositoryModel repository, PullRequestDetailModel pullRequest)
        {
            return Observable.Defer(async () =>
            {
                using (var repo = gitService.GetRepository(repository.LocalPath))
                {
                    var branchName = GetLocalBranchesInternal(repository, repo, pullRequest).FirstOrDefault();

                    Log.Assert(branchName != null, "PullRequestService.SwitchToBranch called but no local branch found");

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
                        await MarkBranchAsPullRequest(repo, branchName, pullRequest.BaseRepositoryOwner, pullRequest.Number);
                    }
                }

                return Observable.Return(Unit.Default);
            });
        }

        public IObservable<(string owner, int number)> GetPullRequestForCurrentBranch(LocalRepositoryModel repository)
        {
            return Observable.Defer(async () =>
            {
                using (var repo = gitService.GetRepository(repository.LocalPath))
                {
                    var configKey = string.Format(
                        CultureInfo.InvariantCulture,
                        "branch.{0}.{1}",
                        repo.Head.FriendlyName,
                        SettingGHfVSPullRequest);
                    var value = await gitClient.GetConfig<string>(repo, configKey);
                    return Observable.Return(ParseGHfVSConfigKeyValue(value));
                }
            });
        }

        public async Task<string> ExtractToTempFile(
            LocalRepositoryModel repository,
            PullRequestDetailModel pullRequest,
            string relativePath,
            string commitSha,
            Encoding encoding)
        {
            var tempFilePath = CalculateTempFileName(relativePath, commitSha, encoding);

            if (!File.Exists(tempFilePath))
            {
                using (var repo = gitService.GetRepository(repository.LocalPath))
                {
                    var remote = await gitClient.GetHttpRemote(repo, "origin");
                    await ExtractToTempFile(repo, pullRequest.Number, commitSha, relativePath, encoding, tempFilePath);
                }
            }

            return tempFilePath;
        }

        public Encoding GetEncoding(LocalRepositoryModel repository, string relativePath)
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

            return null;
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

        public IObservable<Unit> RemoveUnusedRemotes(LocalRepositoryModel repository)
        {
            return Observable.Defer(async () =>
            {
                using (var repo = gitService.GetRepository(repository.LocalPath))
                {
                    var usedRemotes = new HashSet<string>(
                        repo.Branches
                            .Where(x => !x.IsRemote && x.RemoteName != null)
                            .Select(x => x.RemoteName));

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
                }
            });
        }

        /// <inheritdoc />
        public bool ConfirmCancelPendingReview()
        {
            return MessageBox.Show(
                       Resources.CancelPendingReviewConfirmation,
                       Resources.CancelPendingReviewConfirmationCaption,
                       MessageBoxButtons.YesNo,
                       MessageBoxIcon.Question) == DialogResult.Yes;
        }

        async Task<string> CreateRemote(IRepository repo, UriString cloneUri)
        {
            foreach (var remote in repo.Network.Remotes)
            {
                if (UriString.RepositoryUrlsAreEqual(new UriString(remote.Url), cloneUri))
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

        async Task ExtractToTempFile(
            IRepository repo,
            int pullRequestNumber,
            string commitSha,
            string relativePath,
            Encoding encoding,
            string tempFilePath)
        {
            string contents;

            try
            {
                contents = await gitClient.ExtractFile(repo, commitSha, relativePath) ?? string.Empty;
            }
            catch (FileNotFoundException)
            {
                var pullHeadRef = $"refs/pull/{pullRequestNumber}/head";
                var remote = await gitClient.GetHttpRemote(repo, "origin");
                await gitClient.Fetch(repo, remote.Name, commitSha, pullHeadRef);
                contents = await gitClient.ExtractFile(repo, commitSha, relativePath) ?? string.Empty;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath));

            if (encoding != null)
            {
                File.WriteAllText(tempFilePath, contents, encoding);
            }
            else
            {
                File.WriteAllText(tempFilePath, contents);
            }
        }

        IEnumerable<string> GetLocalBranchesInternal(
            LocalRepositoryModel localRepository,
            IRepository repository,
            PullRequestDetailModel pullRequest)
        {
            if (IsPullRequestFromRepository(localRepository, pullRequest))
            {
                return new[] { pullRequest.HeadRefName };
            }
            else
            {
                var key = BuildGHfVSConfigKeyValue(pullRequest.BaseRepositoryOwner, pullRequest.Number);

                return repository.Config
                    .Select(x => new { Branch = BranchCapture.Match(x.Key).Groups["branch"].Value, Value = x.Value })
                    .Where(x => !string.IsNullOrWhiteSpace(x.Branch) && x.Value == key)
                    .Select(x => x.Branch);
            }
        }

        async Task<bool> IsBranchMarkedAsPullRequest(IRepository repo, string branchName, PullRequestDetailModel pullRequest)
        {
            var prConfigKey = $"branch.{branchName}.{SettingGHfVSPullRequest}";
            var value = ParseGHfVSConfigKeyValue(await gitClient.GetConfig<string>(repo, prConfigKey));
            return value != default &&
                value.Item1 == pullRequest.BaseRepositoryOwner &&
                value.Item2 == pullRequest.Number;
        }

        async Task MarkBranchAsPullRequest(IRepository repo, string branchName, string owner, int number)
        {
            // Store the PR number in the branch config with the key "ghfvs-pr".
            var prConfigKey = $"branch.{branchName}.{SettingGHfVSPullRequest}";
            await gitClient.SetConfig(repo, prConfigKey, BuildGHfVSConfigKeyValue(owner, number));
        }

        async Task<IPullRequestModel> PushAndCreatePR(IModelService modelService,
            LocalRepositoryModel sourceRepository, RepositoryModel targetRepository,
            BranchModel sourceBranch, BranchModel targetBranch,
            string title, string body)
        {
            // PullRequestModel doesn't keep a reference to repo
            using (var repo = await Task.Run(() => gitService.GetRepository(sourceRepository.LocalPath)))
            {
                var remote = await gitClient.GetHttpRemote(repo, "origin");
                await gitClient.Push(repo, sourceBranch.Name, remote.Name);

                if (!repo.Branches[sourceBranch.Name].IsTracking)
                    await gitClient.SetTrackingBranch(repo, sourceBranch.Name, remote.Name);

                // delay things a bit to avoid a race between pushing a new branch and creating a PR on it
                if (!Splat.ModeDetector.InUnitTestRunner())
                    await Task.Delay(TimeSpan.FromSeconds(5));

                var ret = await modelService.CreatePullRequest(sourceRepository, targetRepository, sourceBranch, targetBranch, title, body);
                await MarkBranchAsPullRequest(repo, sourceBranch.Name, targetRepository.CloneUrl.Owner, ret.Number);
                gitExt.RefreshActiveRepositories();
                await usageTracker.IncrementCounter(x => x.NumberOfUpstreamPullRequests);
                return ret;
            }
        }

        static string GetSafeBranchName(string name)
        {
            var before = InvalidBranchCharsRegex.Replace(name, "-").TrimEnd('-');

            for (; ; )
            {
                string after = before.Replace("--", "-");

                if (after == before)
                {
                    return before.ToLower(CultureInfo.CurrentCulture);
                }

                before = after;
            }
        }

        static string CalculateTempFileName(string relativePath, string commitSha, Encoding encoding)
        {
            // The combination of relative path, commit SHA and encoding should be sufficient to uniquely identify a file.
            var relativeDir = Path.GetDirectoryName(relativePath) ?? string.Empty;
            var key = relativeDir + '|' + (encoding?.WebName ?? "unknown");
            var relativePathHash = key.GetSha256Hash();
            var tempDir = Path.Combine(Path.GetTempPath(), "GitHubVisualStudio", "FileContents", relativePathHash);
            var tempFileName = Invariant($"{Path.GetFileNameWithoutExtension(relativePath)}@{commitSha}{Path.GetExtension(relativePath)}");
            return Path.Combine(tempDir, tempFileName);
        }

        static string BuildGHfVSConfigKeyValue(string owner, int number)
        {
            return owner + '#' + number.ToString(CultureInfo.InvariantCulture);
        }

        static (string owner, int number) ParseGHfVSConfigKeyValue(string value)
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
                        return (owner, number);
                    }
                }
            }

            return default;
        }

        class ListItemAdapter : PullRequestListItemModel
        {
            public IList<ReviewAdapter> Reviews { get; set; }

            public LastCommitSummaryAdapter LastCommit { get; set; }
        }

        class ReviewAdapter
        {
            public string Body { get; set; }
            public int CommentCount { get; set; }
            public int Count => CommentCount + (!string.IsNullOrWhiteSpace(Body) ? 1 : 0);
        }

        class LastCommitSummaryAdapter
        {
            public List<CheckSuiteSummaryModel> CheckSuites { get; set; }

            public List<StatusSummaryModel> Statuses { get; set; }
        }

        class CheckSuiteSummaryModel
        {
            public List<CheckRunSummaryModel> CheckRuns { get; set; }
        }

        class CheckRunSummaryModel
        {
            public CheckConclusionState? Conclusion { get; set; }
            public CheckStatusState Status { get; set; }
        }

        class StatusSummaryModel
        {
            public StatusState State { get; set; }
        }
    }
}
