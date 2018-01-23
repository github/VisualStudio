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
using NUnit.Framework;

namespace UnitTests.GitHub.App.ViewModels.GitHubPane
{
    public class PullRequestDetailViewModelTests : TestBaseClass
    {
        static readonly Uri Uri = new Uri("http://foo");

        public class TheBodyProperty
        {
            [Test]
            public async Task ShouldUsePlaceholderBodyIfNoneExists()
            {
                var target = CreateTarget();

                await target.Load(CreatePullRequest(body: string.Empty));

                Assert.That("*No description provided.*", Is.EqualTo(target.Body));
            }
        }

        public class TheHeadProperty
        {
            [Test]
            public async Task ShouldAcceptNullHead()
            {
                var target = CreateTarget();
                var model = CreatePullRequest();

                // PullRequest.Head can be null for example if a user deletes the repository after creating the PR.
                model.Head = null;

                await target.Load(model);

                Assert.That("[invalid]", Is.EqualTo(target.SourceBranchDisplayName));
            }
        }

        public class TheChangedFilesTreeProperty
        {
            [Test]
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

                Assert.That(3, Is.EqualTo(target.ChangedFilesTree.Count));

                var dir1 = (PullRequestDirectoryNode)target.ChangedFilesTree[0];
                Assert.That("dir1", Is.EqualTo(dir1.DirectoryName));
                Assert.That(2, Is.EqualTo(dir1.Files.Count));
                Assert.That(1, Is.EqualTo(dir1.Directories.Count));
                Assert.That("f1.cs", Is.EqualTo(dir1.Files[0].FileName));
                Assert.That("f2.cs", Is.EqualTo(dir1.Files[1].FileName));
                Assert.That("dir1", Is.EqualTo(dir1.Files[0].DirectoryPath));
                Assert.That("dir1", Is.EqualTo(dir1.Files[1].DirectoryPath));

                var dir1a = (PullRequestDirectoryNode)dir1.Directories[0];
                Assert.That("dir1a", Is.EqualTo(dir1a.DirectoryName));
                Assert.That(1, Is.EqualTo(dir1a.Files.Count));
                Assert.That(0, Is.EqualTo(dir1a.Directories.Count));

                var dir2 = (PullRequestDirectoryNode)target.ChangedFilesTree[1];
                Assert.That("dir2", Is.EqualTo(dir2.DirectoryName));
                Assert.That(1, Is.EqualTo(dir2.Files.Count));
                Assert.That(0, Is.EqualTo(dir2.Directories.Count));

                var readme = (PullRequestFileNode)target.ChangedFilesTree[2];
                Assert.That("readme.md", Is.EqualTo(readme.FileName));
            }

            [Test]
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
                Assert.That(1, Is.EqualTo(((IPullRequestFileNode)target.ChangedFilesTree[0]).CommentCount));

                file.InlineCommentThreads.Returns(new[] { thread1, thread2 });
                RaisePropertyChanged(file, nameof(file.InlineCommentThreads));
                Assert.That(2, Is.EqualTo(((IPullRequestFileNode)target.ChangedFilesTree[0]).CommentCount));

                // Outdated comment is not included in the count.
                file.InlineCommentThreads.Returns(new[] { thread1, thread2, outdatedThread });
                RaisePropertyChanged(file, nameof(file.InlineCommentThreads));
                Assert.That(2, Is.EqualTo(((IPullRequestFileNode)target.ChangedFilesTree[0]).CommentCount));

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
            [Test]
            public async Task CheckedOutAndUpToDate()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequest());

                Assert.False(target.Checkout.CanExecute(null));
                Assert.That(target.CheckoutState, Is.Null);
            }

            [Test]
            public async Task NotCheckedOut()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequest());

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

                await target.Load(CreatePullRequest());

                Assert.False(target.Checkout.CanExecute(null));
                Assert.That("Cannot checkout as your working directory has uncommitted changes.", Is.EqualTo(target.CheckoutState.ToolTip));
            }

            [Test]
            public async Task CheckoutExistingLocalBranch()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequest(number: 123));

                Assert.True(target.Checkout.CanExecute(null));
                Assert.That("Checkout pr/123", Is.EqualTo(target.CheckoutState.Caption));
            }

            [Test]
            public async Task CheckoutNonExistingLocalBranch()
            {
                var target = CreateTarget(
                    currentBranch: "master");

                await target.Load(CreatePullRequest(number: 123));

                Assert.True(target.Checkout.CanExecute(null));
                Assert.That("Checkout to pr/123", Is.EqualTo(target.CheckoutState.Caption));
            }

            [Test]
            public async Task UpdatesOperationErrorWithExceptionMessage()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");
                var pr = CreatePullRequest();

                pr.Head = new GitReferenceModel("source", null, "sha", (string)null);

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

                await target.Load(CreatePullRequest());

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

                await target.Load(CreatePullRequest());

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

                await target.Load(CreatePullRequest());

                Assert.True(target.Checkout.CanExecute(null));
                Assert.ThrowsAsync<FileNotFoundException>(async () => await target.Checkout.ExecuteAsyncTask());
                Assert.That("Switch threw", Is.EqualTo(target.OperationError));

                await target.Refresh();
                Assert.That(target.OperationError, Is.Null);
            }
        }

        public class ThePullCommand
        {
            [Test]
            public async Task NotCheckedOut()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequest());

                Assert.False(target.Pull.CanExecute(null));
                Assert.That(target.UpdateState, Is.Null);
            }

            [Test]
            public async Task CheckedOutAndUpToDate()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequest());

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

                await target.Load(CreatePullRequest());

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

                await target.Load(CreatePullRequest());

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

                await target.Load(CreatePullRequest());

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

                await target.Load(CreatePullRequest());

                Assert.ThrowsAsync<FileNotFoundException>(() => target.Pull.ExecuteAsyncTask(null));
                Assert.That("Pull threw", Is.EqualTo(target.OperationError));
            }
        }

        public class ThePushCommand
        {
            [Test]
            public async Task NotCheckedOut()
            {
                var target = CreateTarget(
                    currentBranch: "master",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequest());

                Assert.False(target.Push.CanExecute(null));
                Assert.That(target.UpdateState, Is.Null);
            }

            [Test]
            public async Task CheckedOutAndUpToDate()
            {
                var target = CreateTarget(
                    currentBranch: "pr/123",
                    existingPrBranch: "pr/123");

                await target.Load(CreatePullRequest());

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

                await target.Load(CreatePullRequest());

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

                await target.Load(CreatePullRequest());

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

                await target.Load(CreatePullRequest());

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

                await target.Load(CreatePullRequest());

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

                await target.Load(CreatePullRequest());

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
                Substitute.For<IUsageTracker>(),
                Substitute.For<ITeamExplorerContext>());
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
