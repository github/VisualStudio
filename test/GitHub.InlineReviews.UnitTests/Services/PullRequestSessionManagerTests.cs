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
            service.GetPullRequestForCurrentBranch(null).ReturnsForAnyArgs(Observable.Empty<int>());

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

            service.GetPullRequestForCurrentBranch(null).ReturnsForAnyArgs(Observable.Return(22));
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

        IPullRequestModel CreatePullRequestModel(int number)
        {
            var result = Substitute.For<IPullRequestModel>();
            result.Number.Returns(number);
            return result;
        }

        IPullRequestService CreatePullRequestService()
        {
            var result = Substitute.For<IPullRequestService>();
            result.GetPullRequestForCurrentBranch(null).ReturnsForAnyArgs(Observable.Return(CurrentBranchPullRequestNumber));
            return result;
        }

        IRepositoryHosts CreateRepositoryHosts()
        {
            var modelService = Substitute.For<IModelService>();
            modelService.GetPullRequest(null, 0).ReturnsForAnyArgs(x =>
            {
                var pr = Substitute.For<IPullRequestModel>();
                pr.Number.Returns(x.Arg<int>());
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
            result.CloneUrl.Returns(new UriString(cloneUrl));
            return result;
        }
    }
}
