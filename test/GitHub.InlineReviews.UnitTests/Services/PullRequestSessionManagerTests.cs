using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.UnitTests.TestDoubles;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using NSubstitute;
using Xunit;

namespace GitHub.InlineReviews.UnitTests.Services
{
    public class PullRequestSessionManagerTests
    {
        const int CurrentBranchPullRequestNumber = 15;
        const int NotCurrentBranchPullRequestNumber = 10;

        public PullRequestSessionManagerTests()
        {
            Splat.ModeDetector.Current.SetInUnitTestRunner(true);
        }

        [Fact]
        public void CreatesSessionForCurrentBranch()
        {
            var target = new PullRequestSessionManager(
                CreatePullRequestService(),
                Substitute.For<IPullRequestSessionService>(),
                CreateRepositoryHosts(),
                new FakeTeamExplorerServiceHolder(CreateRepositoryModel()));

            Assert.NotNull(target.CurrentSession);
            Assert.True(target.CurrentSession.IsCheckedOut);
        }

        [Fact]
        public void CurrentSessionIsNullIfNoPullRequestForCurrentBranch()
        {
            var service = CreatePullRequestService();
            service.GetPullRequestForCurrentBranch(null).ReturnsForAnyArgs(Observable.Empty<Tuple<string, int>>());

            var target = new PullRequestSessionManager(
                service,
                Substitute.For<IPullRequestSessionService>(),
                CreateRepositoryHosts(),
                new FakeTeamExplorerServiceHolder(CreateRepositoryModel()));

            Assert.Null(target.CurrentSession);
        }

        [Fact]
        public void CurrentSessionChangesWhenBranchChanges()
        {
            var service = CreatePullRequestService();
            var teService = new FakeTeamExplorerServiceHolder(CreateRepositoryModel());
            var target = new PullRequestSessionManager(
                service,
                Substitute.For<IPullRequestSessionService>(),
                CreateRepositoryHosts(),
                teService);

            var session = target.CurrentSession;

            service.GetPullRequestForCurrentBranch(null).ReturnsForAnyArgs(Observable.Return(Tuple.Create("foo", 22)));
            teService.NotifyActiveRepoChanged();

            Assert.NotSame(session, target.CurrentSession);
        }

        [Fact]
        public void CurrentSessionChangesWhenRepoChanged()
        {
            var teService = new FakeTeamExplorerServiceHolder(CreateRepositoryModel());
            var target = new PullRequestSessionManager(
                CreatePullRequestService(),
                Substitute.For<IPullRequestSessionService>(),
                CreateRepositoryHosts(),
                teService);

            var session = target.CurrentSession;

            teService.ActiveRepo = CreateRepositoryModel("https://github.com/owner/other");

            Assert.NotSame(session, target.CurrentSession);
        }

        [Fact]
        public void RepoChangedDoesntCreateNewSessionIfNotNecessary()
        {
            var teService = new FakeTeamExplorerServiceHolder(CreateRepositoryModel());
            var target = new PullRequestSessionManager(
                CreatePullRequestService(),
                Substitute.For<IPullRequestSessionService>(),
                CreateRepositoryHosts(),
                teService);

            var session = target.CurrentSession;

            teService.NotifyActiveRepoChanged();

            Assert.Same(session, target.CurrentSession);
        }

        [Fact]
        public void RepoChangedHandlesNullRepository()
        {
            var teService = new FakeTeamExplorerServiceHolder(CreateRepositoryModel());
            var target = new PullRequestSessionManager(
                CreatePullRequestService(),
                Substitute.For<IPullRequestSessionService>(),
                CreateRepositoryHosts(),
                teService);

            teService.ActiveRepo = null;

            Assert.Null(target.CurrentSession);
        }

        [Fact]
        public async Task GetSessionReturnsAndUpdatesCurrentSessionIfNumbersMatch()
        {
            var target = new PullRequestSessionManager(
                CreatePullRequestService(),
                Substitute.For<IPullRequestSessionService>(),
                CreateRepositoryHosts(),
                new FakeTeamExplorerServiceHolder(CreateRepositoryModel()));

            var newModel = CreatePullRequestModel(CurrentBranchPullRequestNumber);
            var result = await target.GetSession(newModel);

            Assert.Same(target.CurrentSession, result);
            Assert.Same(target.CurrentSession.PullRequest, newModel);
        }
        
        [Fact]
        public async Task GetSessionReturnsNewSessionForPullRequestWithDifferentNumber()
        {
            var target = new PullRequestSessionManager(
                CreatePullRequestService(),
                Substitute.For<IPullRequestSessionService>(),
                CreateRepositoryHosts(),
                new FakeTeamExplorerServiceHolder(CreateRepositoryModel()));

            var newModel = CreatePullRequestModel(NotCurrentBranchPullRequestNumber);
            var result = await target.GetSession(newModel);

            Assert.NotSame(target.CurrentSession, result);
            Assert.Same(result.PullRequest, newModel);
            Assert.False(result.IsCheckedOut);
        }

        [Fact]
        public async Task GetSessionReturnsNewSessionForPullRequestWithDifferentBaseOwner()
        {
            var target = new PullRequestSessionManager(
                CreatePullRequestService(),
                Substitute.For<IPullRequestSessionService>(),
                CreateRepositoryHosts(),
                new FakeTeamExplorerServiceHolder(CreateRepositoryModel()));

            var newModel = CreatePullRequestModel(CurrentBranchPullRequestNumber, "https://github.com/fork/repo");
            var result = await target.GetSession(newModel);

            Assert.NotSame(target.CurrentSession, result);
            Assert.Same(result.PullRequest, newModel);
            Assert.False(result.IsCheckedOut);
        }

        [Fact]
        public async Task GetSessionReturnsSameSessionEachTime()
        {
            var target = new PullRequestSessionManager(
                CreatePullRequestService(),
                Substitute.For<IPullRequestSessionService>(),
                CreateRepositoryHosts(),
                new FakeTeamExplorerServiceHolder(CreateRepositoryModel()));

            var newModel = CreatePullRequestModel(NotCurrentBranchPullRequestNumber);
            var result1 = await target.GetSession(newModel);
            var result2 = await target.GetSession(newModel);

            Assert.Same(result1, result2);
        }

        [Fact]
        public async Task SessionCanBeCollected()
        {
            WeakReference<IPullRequestSession> weakSession = null;

            var target = new PullRequestSessionManager(
                CreatePullRequestService(),
                Substitute.For<IPullRequestSessionService>(),
                CreateRepositoryHosts(),
                new FakeTeamExplorerServiceHolder(CreateRepositoryModel()));

            Func<Task> run = async () =>
            {
                var newModel = CreatePullRequestModel(NotCurrentBranchPullRequestNumber);
                var session = await target.GetSession(newModel);

                Assert.NotNull(session);

                weakSession = new WeakReference<IPullRequestSession>(session);
            };

            await run();
            GC.Collect();

            IPullRequestSession result;
            weakSession.TryGetTarget(out result);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetSessionUpdatesCurrentSessionIfCurrentBranchIsPullRequestButWasNotMarked()
        {
            var service = CreatePullRequestService();

            service.GetPullRequestForCurrentBranch(null).ReturnsForAnyArgs(Observable.Empty<Tuple<string, int>>());

            var target = new PullRequestSessionManager(
                service,
                Substitute.For<IPullRequestSessionService>(),
                CreateRepositoryHosts(),
                new FakeTeamExplorerServiceHolder(CreateRepositoryModel()));
             
            Assert.Null(target.CurrentSession);

            var model = CreatePullRequestModel(CurrentBranchPullRequestNumber);

            service.EnsureLocalBranchesAreMarkedAsPullRequests(Arg.Any<ILocalRepositoryModel>(), model).Returns(Observable.Return(true));
            service.GetPullRequestForCurrentBranch(null).ReturnsForAnyArgs(Observable.Return(Tuple.Create("owner", CurrentBranchPullRequestNumber)));

            var session = await target.GetSession(model);

            Assert.Same(session, target.CurrentSession);
        }

        [Fact]
        public void ReadsPullRequestFromCorrectFork()
        {
            var service = CreatePullRequestService();
            service.GetPullRequestForCurrentBranch(null).ReturnsForAnyArgs(
                Observable.Return(Tuple.Create("fork", CurrentBranchPullRequestNumber)));

            var repositoryHosts = CreateRepositoryHosts();
            var repositoryModel = CreateRepositoryModel();
            var target = new PullRequestSessionManager(
                service,
                Substitute.For<IPullRequestSessionService>(),
                repositoryHosts,
                new FakeTeamExplorerServiceHolder(repositoryModel));

            var modelService = repositoryHosts.LookupHost(HostAddress.Create(repositoryModel.CloneUrl)).ModelService;
            modelService.Received(1).GetPullRequest("fork", "repo", 15);
        }

        [Fact]
        public void CreatesSessionWithCorrectRepositoryOwner()
        {
            var target = new PullRequestSessionManager(
                CreatePullRequestService("this-owner"),
                Substitute.For<IPullRequestSessionService>(),
                CreateRepositoryHosts(),
                new FakeTeamExplorerServiceHolder(CreateRepositoryModel()));

            Assert.Equal("this-owner", target.CurrentSession.RepositoryOwner);
        }

        IPullRequestModel CreatePullRequestModel(int number, string cloneUrl = "https://github.com/owner/repo")
        {
            var result = Substitute.For<IPullRequestModel>();
            result.Number.Returns(number);
            result.Base.Returns(new GitReferenceModel("BASEREF", "pr", "BASESHA", cloneUrl));
            return result;
        }

        IPullRequestService CreatePullRequestService(string owner = "owner")
        {
            var result = Substitute.For<IPullRequestService>();
            result.GetPullRequestForCurrentBranch(null).ReturnsForAnyArgs(Observable.Return(Tuple.Create(owner, CurrentBranchPullRequestNumber)));
            return result;
        }

        IRepositoryHosts CreateRepositoryHosts()
        {
            var modelService = Substitute.For<IModelService>();
            modelService.GetPullRequest(null, null, 0).ReturnsForAnyArgs(x =>
            {
                var cloneUrl = $"https://github.com/{x.ArgAt<string>(0)}/{x.ArgAt<string>(1)}";
                var pr = CreatePullRequestModel(x.ArgAt<int>(2), cloneUrl);
                return Observable.Return(pr);
            });

            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.Returns(modelService);

            var result = Substitute.For<IRepositoryHosts>();
            result.LookupHost(null).ReturnsForAnyArgs(repositoryHost);
            return result;
        }

        ILocalRepositoryModel CreateRepositoryModel(string cloneUrl = "https://github.com/owner/repo")
        {
            var result = Substitute.For<ILocalRepositoryModel>();
            var uriString = new UriString(cloneUrl);
            result.CloneUrl.Returns(uriString);
            result.Name.Returns(uriString.RepositoryName);
            result.Owner.Returns(uriString.Owner);
            return result;
        }
    }
}
