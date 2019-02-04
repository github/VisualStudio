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
using System.Windows.Input;
using System.Reactive.Threading.Tasks;

namespace UnitTests.GitHub.App.ViewModels.GitHubPane
{
    public class PullRequestDetailViewModelTests
    {
        static readonly Uri Uri = new Uri("http://foo");

        public class TheBodyProperty
        {
            [Test]
            public async Task ShouldUsePlaceholderBodyIfNoneExistsAsync()
            {
                var target = CreateTarget();

                await target.Load(CreatePullRequestModel());

                Assert.That("*No description provided.*", Is.EqualTo(target.Body));
            }
        }

        public class TheSourceBranchDisplayNameProperty : TestBaseClass
        {
            [Test]
            public async Task ShouldAcceptNullHeadAsync()
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
            public async Task ShouldShowLatestAcceptedOrChangesRequestedReviewAsync()
            {
                var dateTimeOffset = DateTimeOffset.Now;
                var target = CreateTarget();
                var model = CreatePullRequestModel(
                    CreatePullRequestReviewModel("1", "grokys", PullRequestReviewState.ChangesRequested, dateTimeOffset.AddMinutes(1)),
                    CreatePullRequestReviewModel("2", "shana", PullRequestReviewState.ChangesRequested, dateTimeOffset.AddMinutes(2)),
                    CreatePullRequestReviewModel("3", "grokys", PullRequestReviewState.Approved, dateTimeOffset.AddMinutes(3)),
                    CreatePullRequestReviewModel("4", "grokys", PullRequestReviewState.Commented, dateTimeOffset.AddMinutes(4)));

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
            public async Task ShouldShowLatestCommentedReviewIfNothingElsePresentAsync()
            {
                var dateTimeOffset = DateTimeOffset.Now;
                var target = CreateTarget();
                var model = CreatePullRequestModel(
                    CreatePullRequestReviewModel("1", "shana", PullRequestReviewState.Commented, dateTimeOffset.AddMinutes(1)),
                    CreatePullRequestReviewModel("2", "shana", PullRequestReviewState.Commented, dateTimeOffset.AddMinutes(2)));

                await target.Load(model);

                Assert.That(target.Reviews, Has.Count.EqualTo(2));
                Assert.That(target.Reviews[0].User.Login, Is.EqualTo("shana"));
                Assert.That(target.Reviews[1].User.Login, Is.EqualTo("grokys"));
                Assert.That(target.Reviews[0].Id, Is.EqualTo("2"));
            }

            [Test]
            public async Task ShouldNotShowStartNewReviewWhenHasPendingReviewAsync()
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
            public async Task ShouldShowPendingReviewOverApprovedAsync()
            {
                var dateTimeOffset = DateTimeOffset.Now;

                var target = CreateTarget();
                var model = CreatePullRequestModel(
                    CreatePullRequestReviewModel("1", "grokys", PullRequestReviewState.Approved, dateTimeOffset.AddMinutes(1)),
                    CreatePullRequestReviewModel("2", "grokys", PullRequestReviewState.Pending));

                await target.Load(model);

                Assert.That(target.Reviews, Has.Count.EqualTo(1));
                Assert.That(target.Reviews[0].User.Login, Is.EqualTo("grokys"));
                Assert.That(target.Reviews[0].Id, Is.EqualTo("2"));
            }

            [Test]
            public async Task ShouldNotShowPendingReviewForOtherUserAsync()
            {
                var target = CreateTarget();
                var model = CreatePullRequestModel(
                    CreatePullRequestReviewModel("1", "shana", PullRequestReviewState.Pending));

                await target.Load(model);

                Assert.That(target.Reviews, Has.Count.EqualTo(1));
                Assert.That(target.Reviews[0].User.Login, Is.EqualTo("grokys"));
                Assert.That(target.Reviews[0].Id, Is.Null);
            }

            [Test]
            public async Task ShouldNotShowChangesRequestedAfterDismissed()
            {
                var dateTimeOffset = DateTimeOffset.Now;

                var target = CreateTarget();
                var model = CreatePullRequestModel(
                    CreatePullRequestReviewModel("1", "shana", PullRequestReviewState.ChangesRequested, dateTimeOffset.AddMinutes(1)),
                    CreatePullRequestReviewModel("2", "shana", PullRequestReviewState.Dismissed, dateTimeOffset.AddMinutes(2)));

                await target.Load(model);

                Assert.That(target.Reviews, Has.Count.EqualTo(2));
                Assert.That(target.Reviews[0].User.Login, Is.EqualTo("shana"));
                Assert.That(target.Reviews[0].State, Is.EqualTo(PullRequestReviewState.Dismissed));
                Assert.That(target.Reviews[1].User.Login, Is.EqualTo("grokys"));
            }

            static PullRequestDetailModel CreatePullRequestModel(
                params PullRequestReviewModel[] reviews)
            {
                return PullRequestDetailViewModelTests.CreatePullRequestModel(reviews: reviews);
            }

            static PullRequestReviewModel CreatePullRequestReviewModel(string id,
                string login,
                PullRequestReviewState state,
                DateTimeOffset? submittedAt = null)
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
                    SubmittedAt = submittedAt
                };
            }
        }

        public class TheCheckoutCommand : TestBaseClass
        {
            [Test]
            public async Task CheckedOutAndUpToDateAsync()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.False(((ICommand)target.Checkout).CanExecute(null));
                Assert.That(target.CheckoutState, Is.Null);
            }

            [Test]
            public async Task NotCheckedOutAsync()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.True(((ICommand)target.Checkout).CanExecute(null));
                Assert.True(target.CheckoutState.IsEnabled);
                Assert.That("Checkout pr/123", Is.EqualTo(target.CheckoutState.ToolTip));
            }

            [Test]
            public async Task NotCheckedOutWithWorkingDirectoryDirtyAsync()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123",
                    dirty: true);

                await target.Load(CreatePullRequestModel());

                Assert.False(((ICommand)target.Checkout).CanExecute(null));
                Assert.That("Cannot checkout as your working directory has uncommitted changes.", Is.EqualTo(target.CheckoutState.ToolTip));
            }

            [Test]
            public async Task CheckoutExistingLocalBranchAsync()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel(number: 123));

                Assert.True(((ICommand)target.Checkout).CanExecute(null));
                Assert.That("Checkout pr/123", Is.EqualTo(target.CheckoutState.Caption));
            }

            [Test]
            public async Task CheckoutNonExistingLocalBranchAsync()
            {
                var target = CreateTarget(
                    currentBranch: "master");

                await target.Load(CreatePullRequestModel(number: 123));

                Assert.True(((ICommand)target.Checkout).CanExecute(null));
                Assert.That("Checkout to pr/123", Is.EqualTo(target.CheckoutState.Caption));
            }

            [Test]
            public async Task UpdatesOperationErrorWithExceptionMessageAsync()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");
                var pr = CreatePullRequestModel();

                pr.HeadRepositoryOwner = null;

                await target.Load(pr);

                Assert.False(((ICommand)target.Checkout).CanExecute(null));
                Assert.That("The source repository is no longer available.", Is.EqualTo(target.CheckoutState.ToolTip));
            }

            [Test]
            public async Task SetsOperationErrorOnCheckoutFailureAsync()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.True(((ICommand)target.Checkout).CanExecute(null));

                Assert.ThrowsAsync<FileNotFoundException>(async () => await target.Checkout.Execute());

                Assert.That("Switch threw", Is.EqualTo(target.OperationError));
            }

            [Test]
            public async Task ClearsOperationErrorOnCheckoutSuccessAsync()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.True(((ICommand)target.Checkout).CanExecute(null));
                Assert.ThrowsAsync<FileNotFoundException>(async () => await target.Checkout.Execute());
                Assert.That("Switch threw", Is.EqualTo(target.OperationError));

                await target.Checkout.Execute();
                Assert.That(target.OperationError, Is.Null);
            }

            [Test]
            public async Task ClearsOperationErrorOnCheckoutRefreshAsync()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.True(((ICommand)target.Checkout).CanExecute(null));
                Assert.ThrowsAsync<FileNotFoundException>(async () => await target.Checkout.Execute());
                Assert.That("Switch threw", Is.EqualTo(target.OperationError));

                await target.Refresh();
                Assert.That(target.OperationError, Is.Null);
            }
        }

        public class ThePullCommand : TestBaseClass
        {
            [Test]
            public async Task NotCheckedOutAsync()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.False(((ICommand)target.Pull).CanExecute(null));
                Assert.That(target.UpdateState, Is.Null);
            }

            [Test]
            public async Task CheckedOutAndUpToDateAsync()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.False(((ICommand)target.Pull).CanExecute(null));
                Assert.That(0, Is.EqualTo(target.UpdateState.CommitsAhead));
                Assert.That(0, Is.EqualTo(target.UpdateState.CommitsBehind));
                Assert.That("No commits to pull", Is.EqualTo(target.UpdateState.PullToolTip));
            }

            [Test]
            public async Task CheckedOutAndBehindAsync()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    behindBy: 2);

                await target.Load(CreatePullRequestModel());

                Assert.True(((ICommand)target.Pull).CanExecute(null));
                Assert.That(0, Is.EqualTo(target.UpdateState.CommitsAhead));
                Assert.That(2, Is.EqualTo(target.UpdateState.CommitsBehind));
                Assert.That("Pull from remote branch baz", Is.EqualTo(target.UpdateState.PullToolTip));
            }

            [Test]
            public async Task CheckedOutAndAheadAndBehindAsync()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    aheadBy: 3,
                    behindBy: 2);

                await target.Load(CreatePullRequestModel());

                Assert.True(((ICommand)target.Pull).CanExecute(null));
                Assert.That(3, Is.EqualTo(target.UpdateState.CommitsAhead));
                Assert.That(2, Is.EqualTo(target.UpdateState.CommitsBehind));
                Assert.That("Pull from remote branch baz", Is.EqualTo(target.UpdateState.PullToolTip));
            }

            [Test]
            public async Task CheckedOutAndBehindForkAsync()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    prFromFork: true,
                    behindBy: 2);

                await target.Load(CreatePullRequestModel());

                Assert.True(((ICommand)target.Pull).CanExecute(null));
                Assert.That(0, Is.EqualTo(target.UpdateState.CommitsAhead));
                Assert.That(2, Is.EqualTo(target.UpdateState.CommitsBehind));
                Assert.That("Pull from fork branch foo:baz", Is.EqualTo(target.UpdateState.PullToolTip));
            }

            [Test]
            public async Task UpdatesOperationErrorWithExceptionMessageAsync()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.ThrowsAsync<FileNotFoundException>(() => target.Pull.Execute().ToTask());
                Assert.That("Pull threw", Is.EqualTo(target.OperationError));
            }
        }

        public class ThePushCommand : TestBaseClass
        {
            [Test]
            public async Task NotCheckedOutAsync()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.False(((ICommand)target.Push).CanExecute(null));
                Assert.That(target.UpdateState, Is.Null);
            }

            [Test]
            public async Task CheckedOutAndUpToDateAsync()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.False(((ICommand)target.Push).CanExecute(null));
                Assert.That(0, Is.EqualTo(target.UpdateState.CommitsAhead));
                Assert.That(0, Is.EqualTo(target.UpdateState.CommitsBehind));
                Assert.That("No commits to push", Is.EqualTo(target.UpdateState.PushToolTip));
            }

            [Test]
            public async Task CheckedOutAndAheadAsync()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    aheadBy: 2);

                await target.Load(CreatePullRequestModel());

                Assert.True(((ICommand)target.Push).CanExecute(null));
                Assert.That(2, Is.EqualTo(target.UpdateState.CommitsAhead));
                Assert.That(0, Is.EqualTo(target.UpdateState.CommitsBehind));
                Assert.That("Push to remote branch baz", Is.EqualTo(target.UpdateState.PushToolTip));
            }

            [Test]
            public async Task CheckedOutAndBehindAsync()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    behindBy: 2);

                await target.Load(CreatePullRequestModel());

                Assert.False(((ICommand)target.Push).CanExecute(null));
                Assert.That(0, Is.EqualTo(target.UpdateState.CommitsAhead));
                Assert.That(2, Is.EqualTo(target.UpdateState.CommitsBehind));
                Assert.That("No commits to push", Is.EqualTo(target.UpdateState.PushToolTip));
            }

            [Test]
            public async Task CheckedOutAndAheadAndBehindAsync()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    aheadBy: 3,
                    behindBy: 2);

                await target.Load(CreatePullRequestModel());

                Assert.False(((ICommand)target.Push).CanExecute(null));
                Assert.That(3, Is.EqualTo(target.UpdateState.CommitsAhead));
                Assert.That(2, Is.EqualTo(target.UpdateState.CommitsBehind));
                Assert.That("You must pull before you can push", Is.EqualTo(target.UpdateState.PushToolTip));
            }

            [Test]
            public async Task CheckedOutAndAheadOfForkAsync()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    prFromFork: true,
                    aheadBy: 2);

                await target.Load(CreatePullRequestModel());

                Assert.True(((ICommand)target.Push).CanExecute(null));
                Assert.That(2, Is.EqualTo(target.UpdateState.CommitsAhead));
                Assert.That(0, Is.EqualTo(target.UpdateState.CommitsBehind));
                Assert.That("Push to fork branch foo:baz", Is.EqualTo(target.UpdateState.PushToolTip));
            }

            [Test]
            public async Task UpdatesOperationErrorWithExceptionMessageAsync()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequestModel());

                Assert.ThrowsAsync<FileNotFoundException>(() => target.Push.Execute().ToTask());
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
            var repository = new LocalRepositoryModel
            {
                CloneUrl = new UriString(Uri.ToString()),
                LocalPath = @"C:\projects\ThisRepo",
                Name = "repo"
            };

            var currentBranchModel = new BranchModel(currentBranch, repository);
            var gitService = Substitute.For<IGitService>();
            gitService.GetBranch(repository).Returns(currentBranchModel);

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
                    .Returns(Observable.Empty<BranchModel>());
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
                Substitute.For<ISyncSubmodulesCommand>(),
                Substitute.For<IViewViewModelFactory>(),
                gitService);
            vm.InitializeAsync(repository, Substitute.For<IConnection>(), "owner", "repo", 1).Wait();

            return Tuple.Create(vm, pullRequestService);
        }

        static PullRequestDetailModel CreatePullRequestModel(
            int number = 1,
            IEnumerable<PullRequestReviewModel> reviews = null)
        {
            var author = Substitute.For<IAccount>();

            reviews = reviews ?? Array.Empty<PullRequestReviewModel>();

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
