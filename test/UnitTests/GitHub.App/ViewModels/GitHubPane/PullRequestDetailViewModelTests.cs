using System;
using System.ComponentModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.ViewModels.GitHubPane;
using LibGit2Sharp;
using NSubstitute;
using Xunit;

namespace UnitTests.GitHub.App.ViewModels.GitHubPane
{
    public class PullRequestDetailViewModelTests : TestBaseClass
    {
        static readonly Uri Uri = new Uri("http://foo");

        public class TheBodyProperty
        {
            [Fact]
            public async Task ShouldUsePlaceholderBodyIfNoneExists()
            {
                var target = CreateTarget();

                await target.Load(CreatePullRequest(body: string.Empty));

                Assert.Equal("*No description provided.*", target.Body);
            }
        }

        public class TheHeadProperty
        {
            [Fact]
            public async Task ShouldAcceptNullHead()
            {
                var target = CreateTarget();
                var model = CreatePullRequest();

                // PullRequest.Head can be null for example if a user deletes the repository after creating the PR.
                model.Head = null;

                await target.Load(model);

                Assert.Equal("[invalid]", target.SourceBranchDisplayName);
            }
        }

        public class TheChangedFilesTreeProperty
        {
            [Fact]
            public async Task ShouldCreateChangesTree()
            {
                var target = CreateTarget();
                var pr = CreatePullRequest();

                pr.ChangedFiles = new[]
                {
                    new PullRequestFileModel("readme.md", "abc", PullRequestFileStatus.Modified),
                    new PullRequestFileModel("dir1/f1.cs", "abc", PullRequestFileStatus.Modified),
                    new PullRequestFileModel("dir1/f2.cs", "abc", PullRequestFileStatus.Modified),
                    new PullRequestFileModel("dir1/dir1a/f3.cs", "abc", PullRequestFileStatus.Modified),
                    new PullRequestFileModel("dir2/f4.cs", "abc", PullRequestFileStatus.Modified),
                };

                await target.Load(pr);

                Assert.Equal(3, target.ChangedFilesTree.Count);

                var dir1 = (PullRequestDirectoryNode)target.ChangedFilesTree[0];
                Assert.Equal("dir1", dir1.DirectoryName);
                Assert.Equal(2, dir1.Files.Count);
                Assert.Equal(1, dir1.Directories.Count);
                Assert.Equal("f1.cs", dir1.Files[0].FileName);
                Assert.Equal("f2.cs", dir1.Files[1].FileName);
                Assert.Equal("dir1", dir1.Files[0].DirectoryPath);
                Assert.Equal("dir1", dir1.Files[1].DirectoryPath);

                var dir1a = (PullRequestDirectoryNode)dir1.Directories[0];
                Assert.Equal("dir1a", dir1a.DirectoryName);
                Assert.Equal(1, dir1a.Files.Count);
                Assert.Equal(0, dir1a.Directories.Count);

                var dir2 = (PullRequestDirectoryNode)target.ChangedFilesTree[1];
                Assert.Equal("dir2", dir2.DirectoryName);
                Assert.Equal(1, dir2.Files.Count);
                Assert.Equal(0, dir2.Directories.Count);

                var readme = (PullRequestFileNode)target.ChangedFilesTree[2];
                Assert.Equal("readme.md", readme.FileName);
            }

            [Fact]
            public async Task FileCommentCountShouldTrackSessionInlineComments()
            {
                var pr = CreatePullRequest();
                var file = Substitute.For<IPullRequestSessionFile>();
                var thread1 = CreateThread(5);
                var thread2 = CreateThread(6);
                var outdatedThread = CreateThread(-1);
                var session = Substitute.For<IPullRequestSession>();
                var sessionManager = Substitute.For<IPullRequestSessionManager>();

                file.InlineCommentThreads.Returns(new[] { thread1 });
                session.GetFile("readme.md").Returns(Task.FromResult(file));
                sessionManager.GetSession(pr).Returns(Task.FromResult(session));

                var target = CreateTarget(sessionManager: sessionManager);

                pr.ChangedFiles = new[]
                {
                    new PullRequestFileModel("readme.md", "abc", PullRequestFileStatus.Modified),
                };

                await target.Load(pr);
                Assert.Equal(1, ((IPullRequestFileNode)target.ChangedFilesTree[0]).CommentCount);

                file.InlineCommentThreads.Returns(new[] { thread1, thread2 });
                RaisePropertyChanged(file, nameof(file.InlineCommentThreads));
                Assert.Equal(2, ((IPullRequestFileNode)target.ChangedFilesTree[0]).CommentCount);

                // Outdated comment is not included in the count.
                file.InlineCommentThreads.Returns(new[] { thread1, thread2, outdatedThread });
                RaisePropertyChanged(file, nameof(file.InlineCommentThreads));
                Assert.Equal(2, ((IPullRequestFileNode)target.ChangedFilesTree[0]).CommentCount);

                file.Received(1).PropertyChanged += Arg.Any<PropertyChangedEventHandler>();
            }

            IInlineCommentThreadModel CreateThread(int lineNumber)
            {
                var result = Substitute.For<IInlineCommentThreadModel>();
                result.LineNumber.Returns(lineNumber);
                return result;
            }

            void RaisePropertyChanged<T>(T o, string propertyName)
                where T : INotifyPropertyChanged
            {
                o.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new PropertyChangedEventArgs(propertyName));
            }

        }

        public class TheCheckoutCommand
        {
            [Fact]
            public async Task CheckedOutAndUpToDate()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequest());

                Assert.False(target.Checkout.CanExecute(null));
                Assert.Null(target.CheckoutState);
            }

            [Fact]
            public async Task NotCheckedOut()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequest());

                Assert.True(target.Checkout.CanExecute(null));
                Assert.True(target.CheckoutState.IsEnabled);
                Assert.Equal("Checkout pr/123", target.CheckoutState.ToolTip);
            }

            [Fact]
            public async Task NotCheckedOutWithWorkingDirectoryDirty()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123",
                    dirty: true);

                await target.Load(CreatePullRequest());

                Assert.False(target.Checkout.CanExecute(null));
                Assert.Equal("Cannot checkout as your working directory has uncommitted changes.", target.CheckoutState.ToolTip);
            }

            [Fact]
            public async Task CheckoutExistingLocalBranch()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequest(number: 123));

                Assert.True(target.Checkout.CanExecute(null));
                Assert.Equal("Checkout pr/123", target.CheckoutState.Caption);
            }

            [Fact]
            public async Task CheckoutNonExistingLocalBranch()
            {
                var target = CreateTarget(
                    currentBranch: "master");

                await target.Load(CreatePullRequest(number: 123));

                Assert.True(target.Checkout.CanExecute(null));
                Assert.Equal("Checkout to pr/123", target.CheckoutState.Caption);
            }

            [Fact]
            public async Task UpdatesOperationErrorWithExceptionMessage()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");
                var pr = CreatePullRequest();

                pr.Head = new GitReferenceModel("source", null, "sha", (string)null);

                await target.Load(pr);

                Assert.False(target.Checkout.CanExecute(null));
                Assert.Equal("The source repository is no longer available.", target.CheckoutState.ToolTip);
            }

            [Fact]
            public async Task SetsOperationErrorOnCheckoutFailure()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequest());

                Assert.True(target.Checkout.CanExecute(null));

                await Assert.ThrowsAsync<FileNotFoundException>(async () => await target.Checkout.ExecuteAsyncTask());

                Assert.Equal("Switch threw", target.OperationError);
            }

            [Fact]
            public async Task ClearsOperationErrorOnCheckoutSuccess()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequest());

                Assert.True(target.Checkout.CanExecute(null));
                await Assert.ThrowsAsync<FileNotFoundException>(async () => await target.Checkout.ExecuteAsyncTask());
                Assert.Equal("Switch threw", target.OperationError);

                await target.Checkout.ExecuteAsync();
                Assert.Null(target.OperationError);
            }

            [Fact]
            public async Task ClearsOperationErrorOnCheckoutRefresh()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequest());

                Assert.True(target.Checkout.CanExecute(null));
                await Assert.ThrowsAsync<FileNotFoundException>(async () => await target.Checkout.ExecuteAsyncTask());
                Assert.Equal("Switch threw", target.OperationError);

                await target.Refresh();
                Assert.Null(target.OperationError);
            }
        }

        public class ThePullCommand
        {
            [Fact]
            public async Task NotCheckedOut()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequest());

                Assert.False(target.Pull.CanExecute(null));
                Assert.Null(target.UpdateState);
            }

            [Fact]
            public async Task CheckedOutAndUpToDate()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequest());

                Assert.False(target.Pull.CanExecute(null));
                Assert.Equal(0, target.UpdateState.CommitsAhead);
                Assert.Equal(0, target.UpdateState.CommitsBehind);
                Assert.Equal("No commits to pull", target.UpdateState.PullToolTip);
            }

            [Fact]
            public async Task CheckedOutAndBehind()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    behindBy: 2);

                await target.Load(CreatePullRequest());

                Assert.True(target.Pull.CanExecute(null));
                Assert.Equal(0, target.UpdateState.CommitsAhead);
                Assert.Equal(2, target.UpdateState.CommitsBehind);
                Assert.Equal("Pull from remote branch baz", target.UpdateState.PullToolTip);
            }

            [Fact]
            public async Task CheckedOutAndAheadAndBehind()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    aheadBy: 3,
                    behindBy: 2);

                await target.Load(CreatePullRequest());

                Assert.True(target.Pull.CanExecute(null));
                Assert.Equal(3, target.UpdateState.CommitsAhead);
                Assert.Equal(2, target.UpdateState.CommitsBehind);
                Assert.Equal("Pull from remote branch baz", target.UpdateState.PullToolTip);
            }

            [Fact]
            public async Task CheckedOutAndBehindFork()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    prFromFork: true,
                    behindBy: 2);

                await target.Load(CreatePullRequest());

                Assert.True(target.Pull.CanExecute(null));
                Assert.Equal(0, target.UpdateState.CommitsAhead);
                Assert.Equal(2, target.UpdateState.CommitsBehind);
                Assert.Equal("Pull from fork branch foo:baz", target.UpdateState.PullToolTip);
            }

            [Fact]
            public async Task UpdatesOperationErrorWithExceptionMessage()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequest());

                await Assert.ThrowsAsync<FileNotFoundException>(() => target.Pull.ExecuteAsyncTask(null));
                Assert.Equal("Pull threw", target.OperationError);
            }
        }

        public class ThePushCommand
        {
            [Fact]
            public async Task NotCheckedOut()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequest());

                Assert.False(target.Push.CanExecute(null));
                Assert.Null(target.UpdateState);
            }

            [Fact]
            public async Task CheckedOutAndUpToDate()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequest());

                Assert.False(target.Push.CanExecute(null));
                Assert.Equal(0, target.UpdateState.CommitsAhead);
                Assert.Equal(0, target.UpdateState.CommitsBehind);
                Assert.Equal("No commits to push", target.UpdateState.PushToolTip);
            }

            [Fact]
            public async Task CheckedOutAndAhead()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    aheadBy: 2);

                await target.Load(CreatePullRequest());

                Assert.True(target.Push.CanExecute(null));
                Assert.Equal(2, target.UpdateState.CommitsAhead);
                Assert.Equal(0, target.UpdateState.CommitsBehind);
                Assert.Equal("Push to remote branch baz", target.UpdateState.PushToolTip);
            }

            [Fact]
            public async Task CheckedOutAndBehind()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    behindBy: 2);

                await target.Load(CreatePullRequest());

                Assert.False(target.Push.CanExecute(null));
                Assert.Equal(0, target.UpdateState.CommitsAhead);
                Assert.Equal(2, target.UpdateState.CommitsBehind);
                Assert.Equal("No commits to push", target.UpdateState.PushToolTip);
            }

            [Fact]
            public async Task CheckedOutAndAheadAndBehind()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    aheadBy: 3,
                    behindBy: 2);

                await target.Load(CreatePullRequest());

                Assert.False(target.Push.CanExecute(null));
                Assert.Equal(3, target.UpdateState.CommitsAhead);
                Assert.Equal(2, target.UpdateState.CommitsBehind);
                Assert.Equal("You must pull before you can push", target.UpdateState.PushToolTip);
            }

            [Fact]
            public async Task CheckedOutAndAheadOfFork()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123",
                    prFromFork: true,
                    aheadBy: 2);

                await target.Load(CreatePullRequest());

                Assert.True(target.Push.CanExecute(null));
                Assert.Equal(2, target.UpdateState.CommitsAhead);
                Assert.Equal(0, target.UpdateState.CommitsBehind);
                Assert.Equal("Push to fork branch foo:baz", target.UpdateState.PushToolTip);
            }

            [Fact]
            public async Task UpdatesOperationErrorWithExceptionMessage()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequest());

                await Assert.ThrowsAsync<FileNotFoundException>(() => target.Push.ExecuteAsyncTask(null));
                Assert.Equal("Push threw", target.OperationError);
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
                pullRequestService.GetLocalBranches(repository, Arg.Any<IPullRequestModel>())
                    .Returns(Observable.Return(existingBranchModel));
            }
            else
            {
                pullRequestService.GetLocalBranches(repository, Arg.Any<IPullRequestModel>())
                    .Returns(Observable.Empty<IBranch>());
            }

            pullRequestService.Checkout(repository, Arg.Any<IPullRequestModel>(), Arg.Any<string>()).Returns(x => Throws("Checkout threw"));
            pullRequestService.GetDefaultLocalBranchName(repository, Arg.Any<int>(), Arg.Any<string>()).Returns(x => Observable.Return($"pr/{x[1]}"));
            pullRequestService.IsPullRequestFromRepository(repository, Arg.Any<IPullRequestModel>()).Returns(!prFromFork);
            pullRequestService.IsWorkingDirectoryClean(repository).Returns(Observable.Return(!dirty));
            pullRequestService.Pull(repository).Returns(x => Throws("Pull threw"));
            pullRequestService.Push(repository).Returns(x => Throws("Push threw"));
            pullRequestService.SwitchToBranch(repository, Arg.Any<IPullRequestModel>())
                .Returns(
                    x => Throws("Switch threw"),
                    _ => Observable.Return(Unit.Default));

            var divergence = Substitute.For<BranchTrackingDetails>();
            divergence.AheadBy.Returns(aheadBy);
            divergence.BehindBy.Returns(behindBy);
            pullRequestService.CalculateHistoryDivergence(repository, Arg.Any<int>())
                .Returns(Observable.Return(divergence));

            var vm = new PullRequestDetailViewModel(
                pullRequestService,
                sessionManager ?? Substitute.For<IPullRequestSessionManager>(),
                Substitute.For<IModelServiceFactory>(),
                Substitute.For<IUsageTracker>());
            vm.InitializeAsync(repository, Substitute.For<IConnection>(), "owner", "repo", 1).Wait();

            return Tuple.Create(vm, pullRequestService);
        }

        static PullRequestModel CreatePullRequest(int number = 1, string body = "PR Body")
        {
            var author = Substitute.For<IAccount>();

            return new PullRequestModel(number, "PR 1", author, DateTimeOffset.Now)
            {
                State = PullRequestStateEnum.Open,
                Body = string.Empty,
                Head = new GitReferenceModel("source", "foo:baz", "sha", "https://github.com/foo/bar.git"),
                Base = new GitReferenceModel("dest", "foo:bar", "sha", "https://github.com/foo/bar.git"),
            };
        }

        static IObservable<Unit> Throws(string message)
        {
            Func<IObserver<Unit>, Action> f = _ => { throw new FileNotFoundException(message); };
            return Observable.Create(f);
        }
    }
}
