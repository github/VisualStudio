using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Commands;
using GitHub.Services;
using GitHub.ViewModels.GitHubPane;
using LibGit2Sharp;
using NSubstitute;
using NUnit.Framework;

namespace UnitTests.GitHub.App.ViewModels.GitHubPane
{
    public class PullRequestDetailViewModelTests
    {
        static readonly Uri Uri = new Uri("http://foo");

        public class TheBodyProperty
        {
            [Test]
            public async Task ShouldUsePlaceholderBodyIfNoneExists()
            {
                var target = CreateTarget();

                await target.Load(CreatePullRequestModel(body: string.Empty));

                Assert.That("*No description provided.*", Is.EqualTo(target.Body));
            }
        }

        public class TheSourceBranchDisplayNameProperty : TestBaseClass
        {
            [Test]
            public async Task ShouldAcceptNullHead()
            {
                var target = CreateTarget();
                var model = CreatePullRequestModel();

                // PullRequest.HeadRepositoryOwner can be null if a user deletes the repository after creating the PR.
                model.HeadRepositoryOwner = null;

                await target.Load(model);

                Assert.That("[invalid]", Is.EqualTo(target.SourceBranchDisplayName));
            }
        }

        public class TheReviewsProperty : TestBaseClass
        {
            [Test]
            public async Task ShouldShowLatestAcceptedOrChangesRequestedReview()
            {
                var target = CreateTarget();
                var model = CreatePullRequestModel(
                    CreatePullRequestReviewModel("1", "grokys", PullRequestReviewState.ChangesRequested),
                    CreatePullRequestReviewModel("2", "shana", PullRequestReviewState.ChangesRequested),
                    CreatePullRequestReviewModel("3", "grokys", PullRequestReviewState.Approved),
                    CreatePullRequestReviewModel("4", "grokys", PullRequestReviewState.Commented));

                await target.Load(model);

                Assert.That(target.Reviews, Has.Count.EqualTo(3));
                Assert.That(target.Reviews[0].User.Login, Is.EqualTo("grokys"));
                Assert.That(target.Reviews[1].User.Login, Is.EqualTo("shana"));
                Assert.That(target.Reviews[2].User.Login, Is.EqualTo("grokys"));
                Assert.That(target.Reviews[0].Id, Is.EqualTo("3"));
                Assert.That(target.Reviews[1].Id, Is.EqualTo("2"));
                Assert.That(target.Reviews[2].Id, Is.Null);
            }

            [Test]
            public async Task ShouldShowLatestCommentedReviewIfNothingElsePresent()
            {
                var target = CreateTarget();
                var model = CreatePullRequestModel(
                    CreatePullRequestReviewModel("1", "shana", PullRequestReviewState.Commented),
                    CreatePullRequestReviewModel("2", "shana", PullRequestReviewState.Commented));

                await target.Load(model);

                Assert.That(target.Reviews, Has.Count.EqualTo(2));
                Assert.That(target.Reviews[0].User.Login, Is.EqualTo("shana"));
                Assert.That(target.Reviews[1].User.Login, Is.EqualTo("grokys"));
                Assert.That(target.Reviews[0].Id, Is.EqualTo("2"));
            }

            [Test]
            public async Task ShouldNotShowStartNewReviewWhenHasPendingReview()
            {
                var target = CreateTarget();
                var model = CreatePullRequestModel(
                    CreatePullRequestReviewModel("1", "grokys", PullRequestReviewState.Pending));

                await target.Load(model);

                Assert.That(target.Reviews, Has.Count.EqualTo(1));
                Assert.That(target.Reviews[0].User.Login, Is.EqualTo("grokys"));
                Assert.That(target.Reviews[0].Id, Is.EqualTo("1"));
            }

            [Test]
            public async Task ShouldShowPendingReviewOverApproved()
            {
                var target = CreateTarget();
                var model = CreatePullRequestModel(
                    CreatePullRequestReviewModel("1", "grokys", PullRequestReviewState.Approved),
                    CreatePullRequestReviewModel("2", "grokys", PullRequestReviewState.Pending));

                await target.Load(model);

                Assert.That(target.Reviews, Has.Count.EqualTo(1));
                Assert.That(target.Reviews[0].User.Login, Is.EqualTo("grokys"));
                Assert.That(target.Reviews[0].Id, Is.EqualTo("2"));
            }

            [Test]
            public async Task ShouldNotShowPendingReviewForOtherUser()
            {
                var target = CreateTarget();
                var model = CreatePullRequestModel(
                    CreatePullRequestReviewModel("1", "shana", PullRequestReviewState.Pending));

                await target.Load(model);

                Assert.That(target.Reviews, Has.Count.EqualTo(1));
                Assert.That(target.Reviews[0].User.Login, Is.EqualTo("grokys"));
                Assert.That(target.Reviews[0].Id, Is.Null);
            }

            static PullRequestDetailModel CreatePullRequestModel(
                params PullRequestReviewModel[] reviews)
            {
                return PullRequestDetailViewModelTests.CreatePullRequestModel(reviews: reviews);
            }

            static PullRequestReviewModel CreatePullRequestReviewModel(
                string id,
                string login,
                PullRequestReviewState state)
            {
                var account = new ActorModel
                {
                    Login = login,
                };

                return new PullRequestReviewModel
                {
                    Id = id,
                    Author = account,
                    State = state,
                };
            }
        }

        public class TheCheckoutCommand : TestBaseClass
        {
            [Test]
            public async Task CheckedOutAndUpToDate()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.False(target.Checkout.CanExecute(null));
                Assert.That(target.CheckoutState, Is.Null);
            }

            [Test]
            public async Task NotCheckedOut()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.True(target.Checkout.CanExecute(null));
                Assert.True(target.CheckoutState.IsEnabled);
                Assert.That("Checkout pr/123", Is.EqualTo(target.CheckoutState.ToolTip));
            }

            [Test]
            public async Task NotCheckedOutWithWorkingDirectoryDirty()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123",
                    dirty: true);

                await target.Load(CreatePullRequestModel());

                Assert.False(target.Checkout.CanExecute(null));
                Assert.That("Cannot checkout as your working directory has uncommitted changes.", Is.EqualTo(target.CheckoutState.ToolTip));
            }

            [Test]
            public async Task CheckoutExistingLocalBranch()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel(number: 123));

                Assert.True(target.Checkout.CanExecute(null));
                Assert.That("Checkout pr/123", Is.EqualTo(target.CheckoutState.Caption));
            }

            [Test]
            public async Task CheckoutNonExistingLocalBranch()
            {
                var target = CreateTarget(
                    currentBranch: "master");

                await target.Load(CreatePullRequestModel(number: 123));

                Assert.True(target.Checkout.CanExecute(null));
                Assert.That("Checkout to pr/123", Is.EqualTo(target.CheckoutState.Caption));
            }

            [Test]
            public async Task UpdatesOperationErrorWithExceptionMessage()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");
                var pr = CreatePullRequestModel();

                pr.HeadRepositoryOwner = null;

                await target.Load(pr);

                Assert.False(target.Checkout.CanExecute(null));
                Assert.That("The source repository is no longer available.", Is.EqualTo(target.CheckoutState.ToolTip));
            }

            [Test]
            public async Task SetsOperationErrorOnCheckoutFailure()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.True(target.Checkout.CanExecute(null));

                Assert.ThrowsAsync<FileNotFoundException>(async () => await target.Checkout.ExecuteAsyncTask());

                Assert.That("Switch threw", Is.EqualTo(target.OperationError));
            }

            [Test]
            public async Task ClearsOperationErrorOnCheckoutSuccess()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.True(target.Checkout.CanExecute(null));
                Assert.ThrowsAsync<FileNotFoundException>(async () => await target.Checkout.ExecuteAsyncTask());
                Assert.That("Switch threw", Is.EqualTo(target.OperationError));

                await target.Checkout.ExecuteAsync();
                Assert.That(target.OperationError, Is.Null);
            }

            [Test]
            public async Task ClearsOperationErrorOnCheckoutRefresh()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.True(target.Checkout.CanExecute(null));
                Assert.ThrowsAsync<FileNotFoundException>(async () => await target.Checkout.ExecuteAsyncTask());
                Assert.That("Switch threw", Is.EqualTo(target.OperationError));

                await target.Refresh();
                Assert.That(target.OperationError, Is.Null);
            }
        }

        public class ThePullCommand : TestBaseClass
        {
            [Test]
            public async Task NotCheckedOut()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.False(target.Pull.CanExecute(null));
                Assert.That(target.UpdateState, Is.Null);
            }

            [Test]
            public async Task CheckedOutAndUpToDate()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.False(target.Pull.CanExecute(null));
                Assert.That(0, Is.EqualTo(target.UpdateState.CommitsAhead));
                Assert.That(0, Is.EqualTo(target.UpdateState.CommitsBehind));
                Assert.That("No commits to pull", Is.EqualTo(target.UpdateState.PullToolTip));
            }

            [Test]
            public async Task CheckedOutAndBehind()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    behindBy: 2);

                await target.Load(CreatePullRequestModel());

                Assert.True(target.Pull.CanExecute(null));
                Assert.That(0, Is.EqualTo(target.UpdateState.CommitsAhead));
                Assert.That(2, Is.EqualTo(target.UpdateState.CommitsBehind));
                Assert.That("Pull from remote branch baz", Is.EqualTo(target.UpdateState.PullToolTip));
            }

            [Test]
            public async Task CheckedOutAndAheadAndBehind()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    aheadBy: 3,
                    behindBy: 2);

                await target.Load(CreatePullRequestModel());

                Assert.True(target.Pull.CanExecute(null));
                Assert.That(3, Is.EqualTo(target.UpdateState.CommitsAhead));
                Assert.That(2, Is.EqualTo(target.UpdateState.CommitsBehind));
                Assert.That("Pull from remote branch baz", Is.EqualTo(target.UpdateState.PullToolTip));
            }

            [Test]
            public async Task CheckedOutAndBehindFork()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    prFromFork: true,
                    behindBy: 2);

                await target.Load(CreatePullRequestModel());

                Assert.True(target.Pull.CanExecute(null));
                Assert.That(0, Is.EqualTo(target.UpdateState.CommitsAhead));
                Assert.That(2, Is.EqualTo(target.UpdateState.CommitsBehind));
                Assert.That("Pull from fork branch foo:baz", Is.EqualTo(target.UpdateState.PullToolTip));
            }

            [Test]
            public async Task UpdatesOperationErrorWithExceptionMessage()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.ThrowsAsync<FileNotFoundException>(() => target.Pull.ExecuteAsyncTask(null));
                Assert.That("Pull threw", Is.EqualTo(target.OperationError));
            }
        }

        public class ThePushCommand : TestBaseClass
        {
            [Test]
            public async Task NotCheckedOut()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.False(target.Push.CanExecute(null));
                Assert.That(target.UpdateState, Is.Null);
            }

            [Test]
            public async Task CheckedOutAndUpToDate()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.False(target.Push.CanExecute(null));
                Assert.That(0, Is.EqualTo(target.UpdateState.CommitsAhead));
                Assert.That(0, Is.EqualTo(target.UpdateState.CommitsBehind));
                Assert.That("No commits to push", Is.EqualTo(target.UpdateState.PushToolTip));
            }

            [Test]
            public async Task CheckedOutAndAhead()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    aheadBy: 2);

                await target.Load(CreatePullRequestModel());

                Assert.True(target.Push.CanExecute(null));
                Assert.That(2, Is.EqualTo(target.UpdateState.CommitsAhead));
                Assert.That(0, Is.EqualTo(target.UpdateState.CommitsBehind));
                Assert.That("Push to remote branch baz", Is.EqualTo(target.UpdateState.PushToolTip));
            }

            [Test]
            public async Task CheckedOutAndBehind()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    behindBy: 2);

                await target.Load(CreatePullRequestModel());

                Assert.False(target.Push.CanExecute(null));
                Assert.That(0, Is.EqualTo(target.UpdateState.CommitsAhead));
                Assert.That(2, Is.EqualTo(target.UpdateState.CommitsBehind));
                Assert.That("No commits to push", Is.EqualTo(target.UpdateState.PushToolTip));
            }

            [Test]
            public async Task CheckedOutAndAheadAndBehind()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    aheadBy: 3,
                    behindBy: 2);

                await target.Load(CreatePullRequestModel());

                Assert.False(target.Push.CanExecute(null));
                Assert.That(3, Is.EqualTo(target.UpdateState.CommitsAhead));
                Assert.That(2, Is.EqualTo(target.UpdateState.CommitsBehind));
                Assert.That("You must pull before you can push", Is.EqualTo(target.UpdateState.PushToolTip));
            }

            [Test]
            public async Task CheckedOutAndAheadOfFork()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    prFromFork: true,
                    aheadBy: 2);

                await target.Load(CreatePullRequestModel());

                Assert.True(target.Push.CanExecute(null));
                Assert.That(2, Is.EqualTo(target.UpdateState.CommitsAhead));
                Assert.That(0, Is.EqualTo(target.UpdateState.CommitsBehind));
                Assert.That("Push to fork branch foo:baz", Is.EqualTo(target.UpdateState.PushToolTip));
            }

            [Test]
            public async Task UpdatesOperationErrorWithExceptionMessage()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.ThrowsAsync<FileNotFoundException>(() => target.Push.ExecuteAsyncTask(null));
                Assert.That("Push threw", Is.EqualTo(target.OperationError));
            }
        }

        static PullRequestDetailViewModel CreateTarget(
            string currentBranch = "master",
            string existingPrBranch = null,
            bool prFromFork = false,
            bool dirty = false,
            int aheadBy = 0,
            int behindBy = 0,
            IPullRequestSessionManager sessionManager = null)
        {
            return CreateTargetAndService(
                currentBranch: currentBranch,
                existingPrBranch: existingPrBranch,
                prFromFork: prFromFork,
                dirty: dirty,
                aheadBy: aheadBy,
                behindBy: behindBy,
                sessionManager: sessionManager).Item1;
        }

        static Tuple<PullRequestDetailViewModel, IPullRequestService> CreateTargetAndService(
            string currentBranch = "master",
            string existingPrBranch = null,
            bool prFromFork = false,
            bool dirty = false,
            int aheadBy = 0,
            int behindBy = 0,
            IPullRequestSessionManager sessionManager = null)
        {
            var repository = Substitute.For<ILocalRepositoryModel>();
            var currentBranchModel = new BranchModel(currentBranch, repository);
            repository.CurrentBranch.Returns(currentBranchModel);
            repository.CloneUrl.Returns(new UriString(Uri.ToString()));
            repository.LocalPath.Returns(@"C:\projects\ThisRepo");
            repository.Name.Returns("repo");

            var pullRequestService = Substitute.For<IPullRequestService>();

            if (existingPrBranch != null)
            {
                var existingBranchModel = new BranchModel(existingPrBranch, repository);
                pullRequestService.GetLocalBranches(repository, Arg.Any<PullRequestDetailModel>())
                    .Returns(Observable.Return(existingBranchModel));
            }
            else
            {
                pullRequestService.GetLocalBranches(repository, Arg.Any<PullRequestDetailModel>())
                    .Returns(Observable.Empty<IBranch>());
            }

            pullRequestService.Checkout(repository, Arg.Any<PullRequestDetailModel>(), Arg.Any<string>()).Returns(x => Throws("Checkout threw"));
            pullRequestService.GetDefaultLocalBranchName(repository, Arg.Any<int>(), Arg.Any<string>()).Returns(x => Observable.Return($"pr/{x[1]}"));
            pullRequestService.IsPullRequestFromRepository(repository, Arg.Any<PullRequestDetailModel>()).Returns(!prFromFork);
            pullRequestService.IsWorkingDirectoryClean(repository).Returns(Observable.Return(!dirty));
            pullRequestService.Pull(repository).Returns(x => Throws("Pull threw"));
            pullRequestService.Push(repository).Returns(x => Throws("Push threw"));
            pullRequestService.SwitchToBranch(repository, Arg.Any<PullRequestDetailModel>())
                .Returns(
                    x => Throws("Switch threw"),
                    _ => Observable.Return(Unit.Default));

            var divergence = Substitute.For<BranchTrackingDetails>();
            divergence.AheadBy.Returns(aheadBy);
            divergence.BehindBy.Returns(behindBy);
            pullRequestService.CalculateHistoryDivergence(repository, Arg.Any<int>())
                .Returns(Observable.Return(divergence));

            if (sessionManager == null)
            {
                var currentSession = Substitute.For<IPullRequestSession>();
                currentSession.PullRequest.Returns(CreatePullRequestModel());
                currentSession.User.Returns(new ActorModel { Login = "grokys" });

                sessionManager = Substitute.For<IPullRequestSessionManager>();
                sessionManager.CurrentSession.Returns(currentSession);
                sessionManager.GetSession("owner", "repo", 1).ReturnsForAnyArgs(currentSession);
            }

            var vm = new PullRequestDetailViewModel(
                pullRequestService,
                sessionManager,
                Substitute.For<IModelServiceFactory>(),
                Substitute.For<IUsageTracker>(),
                Substitute.For<ITeamExplorerContext>(),
                Substitute.For<IPullRequestFilesViewModel>(),
                Substitute.For<ISyncSubmodulesCommand>());
            vm.InitializeAsync(repository, Substitute.For<IConnection>(), "owner", "repo", 1).Wait();

            return Tuple.Create(vm, pullRequestService);
        }

        static PullRequestDetailModel CreatePullRequestModel(
            int number = 1,
            string body = "PR Body",
            IEnumerable<PullRequestReviewModel> reviews = null)
        {
            var author = Substitute.For<IAccount>();

            reviews = reviews ?? new PullRequestReviewModel[0];

            return new PullRequestDetailModel
            {
                Number = number,
                Title = "PR 1",
                Author = new ActorModel(),
                State = PullRequestStateEnum.Open,
                Body = string.Empty,
                BaseRefName = "master",
                BaseRefSha = "BASE_REF",
                HeadRefName = "baz",
                HeadRefSha = "HEAD_REF",
                HeadRepositoryOwner = "foo",
                UpdatedAt = DateTimeOffset.Now,
                Reviews = reviews.ToList(),
            };
        }

        static IObservable<Unit> Throws(string message)
        {
            Func<IObserver<Unit>, Action> f = _ => { throw new FileNotFoundException(message); };
            return Observable.Create(f);
        }
    }
}
