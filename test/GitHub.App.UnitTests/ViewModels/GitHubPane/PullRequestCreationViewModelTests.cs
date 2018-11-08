using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.ViewModels.GitHubPane;
using NSubstitute;
using Octokit;
using UnitTests;
using NUnit.Framework;
using IConnection = GitHub.Models.IConnection;
using System.Reactive.Concurrency;
using GitHub.Models.Drafts;

/// <summary>
/// All the tests in this class are split in subclasses so that when they run
/// in parallel the temp dir is set up uniquely for each test
/// </summary>

public class PullRequestCreationViewModelTests : TestBaseClass
{
    static LibGit2Sharp.IRepository SetupLocalRepoMock(IGitClient gitClient, IGitService gitService, string remote, string head, bool isTracking)
    {
        var l2remote = Substitute.For<LibGit2Sharp.Remote>();
        l2remote.Name.Returns(remote);
        gitClient.GetHttpRemote(Args.LibGit2Repo, Args.String).Returns(Task.FromResult(l2remote));

        var l2repo = Substitute.For<LibGit2Sharp.IRepository>();
        var l2branchcol = Substitute.For<LibGit2Sharp.BranchCollection>();
        var l2branch = Substitute.For<LibGit2Sharp.Branch>();
        l2branch.FriendlyName.Returns(head);
        l2branch.IsTracking.Returns(isTracking);
        l2branchcol[Args.String].Returns(l2branch);
        l2repo.Branches.Returns(l2branchcol);
        l2repo.Head.Returns(l2branch);
        gitService.GetRepository(Args.String).Returns(l2repo);
        return l2repo;
    }

    struct TestData
    {
        public IServiceProvider ServiceProvider;
        public LocalRepositoryModel ActiveRepo;
        public LibGit2Sharp.IRepository L2Repo;
        public RepositoryModel SourceRepo;
        public RepositoryModel TargetRepo;
        public BranchModel SourceBranch;
        public BranchModel TargetBranch;
        public IGitClient GitClient;
        public IGitService GitService;
        public INotificationService NotificationService;
        public IConnection Connection;
        public IApiClient ApiClient;
        public IModelService ModelService;

        public IModelServiceFactory GetModelServiceFactory()
        {
            var result = Substitute.For<IModelServiceFactory>();
            result.CreateAsync(Connection).Returns(ModelService);
            result.CreateBlocking(Connection).Returns(ModelService);
            return result;
        }
    }

    static TestData PrepareTestData(
        string repoName, string sourceRepoOwner, string sourceBranchName,
        string targetRepoOwner, string targetBranchName,
        string remote,
        bool repoIsFork, bool sourceBranchIsTracking)
    {
        var serviceProvider = Substitutes.ServiceProvider;
        var gitService = serviceProvider.GetGitService();
        var gitClient = Substitute.For<IGitClient>();
        var notifications = Substitute.For<INotificationService>();
        var connection = Substitute.For<IConnection>();
        var api = Substitute.For<IApiClient>();
        var ms = Substitute.For<IModelService>();

        connection.HostAddress.Returns(HostAddress.Create("https://github.com"));

        var activeRepo = new LocalRepositoryModel
        {
            LocalPath = "",
            Name = repoName,
            CloneUrl = new UriString("http://github.com/" + sourceRepoOwner + "/" + repoName)
        };

        Repository githubRepoParent = null;
        if (repoIsFork)
            githubRepoParent = CreateRepository(targetRepoOwner, repoName, id: 1);
        var githubRepo = CreateRepository(sourceRepoOwner, repoName, id: 2, parent: githubRepoParent);
        var sourceBranch = new BranchModel(sourceBranchName, activeRepo);
        var sourceRepo = CreateRemoteRepositoryModel(githubRepo);
        var targetRepo = targetRepoOwner == sourceRepoOwner ? sourceRepo : sourceRepo.Parent;
        var targetBranch = targetBranchName != targetRepo.DefaultBranch.Name ? new BranchModel(targetBranchName, targetRepo) : targetRepo.DefaultBranch;

        gitService.GetBranch(activeRepo).Returns(sourceBranch);
        api.GetRepository(Args.String, Args.String).Returns(Observable.Return(githubRepo));
        ms.ApiClient.Returns(api);

        // Default to returning no branches
        ms.GetBranches(null).ReturnsForAnyArgs(Observable.Empty<BranchModel>());

        // sets up the libgit2sharp repo and branch objects
        var l2repo = SetupLocalRepoMock(gitClient, gitService, remote, sourceBranchName, sourceBranchIsTracking);

        return new TestData
        {
            ServiceProvider = serviceProvider,
            ActiveRepo = activeRepo,
            L2Repo = l2repo,
            SourceRepo = sourceRepo,
            SourceBranch = sourceBranch,
            TargetRepo = targetRepo,
            TargetBranch = targetBranch,
            GitClient = gitClient,
            GitService = gitService,
            NotificationService = notifications,
            Connection = connection,
            ApiClient = api,
            ModelService = ms
        };
    }

    static RemoteRepositoryModel CreateRemoteRepositoryModel(Repository repository)
    {
        var ownerAccount = new GitHub.Models.Account(repository.Owner);
        var parent = repository.Parent != null ? CreateRemoteRepositoryModel(repository.Parent) : null;
        var model = new RemoteRepositoryModel(repository.Id, repository.Name, repository.CloneUrl,
            repository.Private, repository.Fork, ownerAccount, parent, repository.DefaultBranch);

        if (parent != null)
        {
            parent.DefaultBranch.DisplayName = parent.DefaultBranch.Id;
        }

        return model;
    }

    [Test]
    public async Task TargetBranchDisplayNameIncludesRepoOwnerWhenForkAsync()
    {
        var data = PrepareTestData("octokit.net", "shana", "master", "octokit", "master", "origin", true, true);
        var prservice = new PullRequestService(data.GitClient, data.GitService, Substitute.For<IVSGitExt>(), Substitute.For<IGraphQLClientFactory>(), data.ServiceProvider.GetOperatingSystem(), Substitute.For<IUsageTracker>());
        prservice.GetPullRequestTemplate(data.ActiveRepo).Returns(Observable.Empty<string>());
        var vm = new PullRequestCreationViewModel(data.GetModelServiceFactory(), prservice, data.NotificationService,
            Substitute.For<IMessageDraftStore>(), data.GitService);
        await vm.InitializeAsync(data.ActiveRepo, data.Connection);
        Assert.That("octokit/master", Is.EqualTo(vm.TargetBranch.DisplayName));
    }

    [TestCase("repo-name-1", "source-repo-owner", "source-branch", true, true, "target-repo-owner", "target-branch", "title", null)]
    [TestCase("repo-name-2", "source-repo-owner", "source-branch", true, true, "target-repo-owner", "master", "title", "description")]
    [TestCase("repo-name-3", "source-repo-owner", "master", true, true, "target-repo-owner", "master", "title", "description")]
    [TestCase("repo-name-4", "source-repo-owner", "source-branch", false, true, "source-repo-owner", "target-branch", "title", null)]
    [TestCase("repo-name-5", "source-repo-owner", "source-branch", false, true, "source-repo-owner", "master", "title", "description")]
    [TestCase("repo-name-6", "source-repo-owner", "source-branch", true, false, "target-repo-owner", "target-branch", "title", null)]
    [TestCase("repo-name-7", "source-repo-owner", "source-branch", true, false, "target-repo-owner", "master", "title", "description")]
    [TestCase("repo-name-8", "source-repo-owner", "master", true, false, "target-repo-owner", "master", "title", "description")]
    [TestCase("repo-name-9", "source-repo-owner", "source-branch", false, false, "source-repo-owner", "target-branch", "title", null)]
    [TestCase("repo-name-10", "source-repo-owner", "source-branch", false, false, "source-repo-owner", "master", "title", "description")]
    [TestCase("repo-name-11", "source-repo-owner", "source-branch", false, false, "source-repo-owner", "master", null, null)]
    public async Task CreatingPRsAsync(
        string repoName, string sourceRepoOwner, string sourceBranchName,
        bool repoIsFork, bool sourceBranchIsTracking,
        string targetRepoOwner, string targetBranchName,
        string title, string body)
    {
        var remote = "origin";
        var data = PrepareTestData(repoName, sourceRepoOwner, sourceBranchName, targetRepoOwner, targetBranchName, "origin",
            repoIsFork, sourceBranchIsTracking);

        var targetRepo = data.TargetRepo;
        var gitClient = data.GitClient;
        var l2repo = data.L2Repo;
        var activeRepo = data.ActiveRepo;
        var sourceBranch = data.SourceBranch;
        var targetBranch = data.TargetBranch;
        var ms = data.ModelService;

        var prservice = new PullRequestService(data.GitClient, data.GitService, Substitute.For<IVSGitExt>(), Substitute.For<IGraphQLClientFactory>(), data.ServiceProvider.GetOperatingSystem(), Substitute.For<IUsageTracker>());
        var vm = new PullRequestCreationViewModel(data.GetModelServiceFactory(), prservice, data.NotificationService,
            Substitute.For<IMessageDraftStore>(), data.GitService);
        await vm.InitializeAsync(data.ActiveRepo, data.Connection);

        // the TargetBranch property gets set to whatever the repo default is (we assume master here),
        // so we only set it manually to emulate the user selecting a different target branch
        if (targetBranchName != "master")
            vm.TargetBranch = new BranchModel(targetBranchName, targetRepo);

        if (title != null)
            vm.PRTitle = title;

        // this is optional
        if (body != null)
            vm.Description = body;

        ms.CreatePullRequest(activeRepo, targetRepo, sourceBranch, targetBranch, Arg.Any<string>(), Arg.Any<string>())
            .Returns(x =>
            {
                var pr = Substitute.For<IPullRequestModel>();
                pr.Base.Returns(new GitReferenceModel("ref", "label", "sha", "https://clone.url"));
                return Observable.Return(pr);
            });

        await vm.CreatePullRequest.Execute();

        var unused2 = gitClient.Received().Push(l2repo, sourceBranchName, remote);
        if (!sourceBranchIsTracking)
            unused2 = gitClient.Received().SetTrackingBranch(l2repo, sourceBranchName, remote);
        else
            unused2 = gitClient.DidNotReceiveWithAnyArgs().SetTrackingBranch(Args.LibGit2Repo, Args.String, Args.String);
        var unused = ms.Received().CreatePullRequest(activeRepo, targetRepo, sourceBranch, targetBranch, title ?? "Source branch", body ?? String.Empty);
    }

    [Test]
    public async Task TemplateIsUsedIfPresentAsync()
    {
        var data = PrepareTestData("stuff", "owner", "master", "owner", "master",
            "origin", false, true);

        var prservice = Substitute.For<IPullRequestService>();
        prservice.GetPullRequestTemplate(data.ActiveRepo).Returns(Observable.Return("Test PR template"));

        var vm = new PullRequestCreationViewModel(data.GetModelServiceFactory(), prservice, data.NotificationService,
            Substitute.For<IMessageDraftStore>(), data.GitService);
        await vm.InitializeAsync(data.ActiveRepo, data.Connection);

        Assert.That("Test PR template", Is.EqualTo(vm.Description));
    }

    [Test]
    public async Task LoadsDraft()
    {
        var data = PrepareTestData("repo", "owner", "feature-branch", "owner", "master", "origin", false, false);
        var draftStore = Substitute.For<IMessageDraftStore>();
        draftStore.GetDraft<PullRequestDraft>("pr|http://github.com/owner/repo|feature-branch", string.Empty)
            .Returns(new PullRequestDraft
            {
                Title = "This is a Title.",
                Body = "This is a PR.",
            });

        var prservice = Substitute.For<IPullRequestService>();
        var vm = new PullRequestCreationViewModel(data.GetModelServiceFactory(), prservice, data.NotificationService,
            draftStore, data.GitService);
        await vm.InitializeAsync(data.ActiveRepo, data.Connection);

        Assert.That(vm.PRTitle, Is.EqualTo("This is a Title."));
        Assert.That(vm.Description, Is.EqualTo("This is a PR."));
    }

    [Test]
    public async Task UpdatesDraftWhenDescriptionChanges()
    {
        var data = PrepareTestData("repo", "owner", "feature-branch", "owner", "master", "origin", false, false);
        var scheduler = new HistoricalScheduler();
        var draftStore = Substitute.For<IMessageDraftStore>();
        var prservice = Substitute.For<IPullRequestService>();
        var vm = new PullRequestCreationViewModel(data.GetModelServiceFactory(), prservice, data.NotificationService,
            draftStore, data.GitService, scheduler);
        await vm.InitializeAsync(data.ActiveRepo, data.Connection);

        vm.Description = "Body changed.";

        await draftStore.DidNotReceiveWithAnyArgs().UpdateDraft<PullRequestDraft>(null, null, null);

        scheduler.AdvanceBy(TimeSpan.FromSeconds(1));

        await draftStore.Received().UpdateDraft(
            "pr|http://github.com/owner/repo|feature-branch",
            string.Empty,
            Arg.Is<PullRequestDraft>(x => x.Body == "Body changed."));
    }

    [Test]
    public async Task UpdatesDraftWhenTitleChanges()
    {
        var data = PrepareTestData("repo", "owner", "feature-branch", "owner", "master", "origin", false, false);
        var scheduler = new HistoricalScheduler();
        var draftStore = Substitute.For<IMessageDraftStore>();
        var prservice = Substitute.For<IPullRequestService>();
        var vm = new PullRequestCreationViewModel(data.GetModelServiceFactory(), prservice, data.NotificationService,
            draftStore, data.GitService, scheduler);
        await vm.InitializeAsync(data.ActiveRepo, data.Connection);

        vm.PRTitle = "Title changed.";

        await draftStore.DidNotReceiveWithAnyArgs().UpdateDraft<PullRequestDraft>(null, null, null);

        scheduler.AdvanceBy(TimeSpan.FromSeconds(1));

        await draftStore.Received().UpdateDraft(
            "pr|http://github.com/owner/repo|feature-branch",
            string.Empty,
            Arg.Is<PullRequestDraft>(x => x.Title == "Title changed."));
    }

    [Test]
    public async Task DeletesDraftWhenPullRequestSubmitted()
    {
        var data = PrepareTestData("repo", "owner", "feature-branch", "owner", "master", "origin", false, false);
        var scheduler = new HistoricalScheduler();
        var draftStore = Substitute.For<IMessageDraftStore>();
        var prservice = Substitute.For<IPullRequestService>();
        var vm = new PullRequestCreationViewModel(data.GetModelServiceFactory(), prservice, data.NotificationService, draftStore,
            data.GitService, scheduler);
        await vm.InitializeAsync(data.ActiveRepo, data.Connection);

        await vm.CreatePullRequest.Execute();

        await draftStore.Received().DeleteDraft("pr|http://github.com/owner/repo|feature-branch", string.Empty);
    }

    [Test]
    public async Task DeletesDraftWhenCanceled()
    {
        var data = PrepareTestData("repo", "owner", "feature-branch", "owner", "master", "origin", false, false);
        var scheduler = new HistoricalScheduler();
        var draftStore = Substitute.For<IMessageDraftStore>();
        var prservice = Substitute.For<IPullRequestService>();
        var vm = new PullRequestCreationViewModel(data.GetModelServiceFactory(), prservice, data.NotificationService, draftStore,
            data.GitService, scheduler);
        await vm.InitializeAsync(data.ActiveRepo, data.Connection);

        await vm.Cancel.Execute();

        await draftStore.Received().DeleteDraft("pr|http://github.com/owner/repo|feature-branch", string.Empty);
    }
}
