using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.Settings;
using GitHub.ViewModels;
using LibGit2Sharp;
using NSubstitute;
using Octokit;
using Xunit;

namespace UnitTests.GitHub.App.ViewModels
{
    public class PullRequestDetailViewModelTests : TestBaseClass
    {
        static readonly Uri Uri = new Uri("http://foo");

        [Fact]
        public async Task ShouldUsePlaceholderBodyIfNoneExists()
        {
            var target = CreateTarget();

            await target.Load(CreatePullRequest(body: string.Empty));

            Assert.Equal("*No description provided.*", target.Body);
        }

        [Fact]
        public async Task ShouldCreateChangesTree()
        {
            var target = CreateTarget();
            var pr = CreatePullRequest();

            pr.ChangedFiles = new[]
            {
                new PullRequestFileModel("readme.md", PullRequestFileStatus.Modified),
                new PullRequestFileModel("dir1/f1.cs", PullRequestFileStatus.Modified),
                new PullRequestFileModel("dir1/f2.cs", PullRequestFileStatus.Modified),
                new PullRequestFileModel("dir1/dir1a/f3.cs", PullRequestFileStatus.Modified),
                new PullRequestFileModel("dir2/f4.cs", PullRequestFileStatus.Modified),
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
        public async Task CheckoutModeShouldBeUpToDate()
        {
            var target = CreateTarget(
                currentBranch: "pr/123",
                existingPrBranch: "pr/123");
            await target.Load(CreatePullRequest());

            Assert.Equal(CheckoutMode.UpToDate, target.CheckoutMode);
            Assert.False(target.Checkout.CanExecute(null));
        }

        [Fact]
        public async Task CheckoutDisabledMessageShouldBeNullWhenUpToDateEvenWhenWorkingDirectoryDirty()
        {
            var target = CreateTarget(
                currentBranch: "pr/123",
                existingPrBranch: "pr/123",
                dirty: true);
            await target.Load(CreatePullRequest());

            Assert.Equal(CheckoutMode.UpToDate, target.CheckoutMode);
            Assert.Null(target.CheckoutDisabledMessage);
        }

        [Fact]
        public async Task CheckoutModeShouldBeNeedsPull()
        {
            var target = CreateTarget(
                currentBranch: "pr/123",
                existingPrBranch: "pr/123",
                behindBy: 3);
            await target.Load(CreatePullRequest());

            Assert.Equal(CheckoutMode.NeedsPull, target.CheckoutMode);
            Assert.Equal(3, target.CommitsBehind);
            Assert.True(target.Checkout.CanExecute(null));
        }

        [Fact]
        public async Task CheckoutDisabledMessageShouldBeSetWhenNeedsPullAndWorkingDirectoryDirty()
        {
            var target = CreateTarget(
                currentBranch: "pr/123",
                existingPrBranch: "pr/123",
                behindBy: 3,
                dirty: true);
            await target.Load(CreatePullRequest());

            Assert.Equal(CheckoutMode.NeedsPull, target.CheckoutMode);
            Assert.Equal("Cannot update branch as your working directory has uncommitted changes.", target.CheckoutDisabledMessage);
            Assert.False(target.Checkout.CanExecute(null));
        }

        [Fact]
        public async Task CheckoutModeShouldBeSwitch()
        {
            var target = CreateTarget(
                currentBranch: "master",
                existingPrBranch: "pr/123");
            await target.Load(CreatePullRequest());

            Assert.Equal(CheckoutMode.Switch, target.CheckoutMode);
            Assert.True(target.Checkout.CanExecute(null));
        }

        [Fact]
        public async Task CheckoutDisabledMessageShouldBeSetWhenNeedsSwitchAndWorkingDirectoryDirty()
        {
            var target = CreateTarget(
                currentBranch: "master",
                existingPrBranch: "pr/123",
                dirty: true);
            await target.Load(CreatePullRequest());

            Assert.Equal(CheckoutMode.Switch, target.CheckoutMode);
            Assert.Equal("Cannot switch branches as your working directory has uncommitted changes.", target.CheckoutDisabledMessage);
            Assert.False(target.Checkout.CanExecute(null));
        }

        [Fact]
        public async Task CheckoutModeShouldBeFetch()
        {
            var target = CreateTarget(currentBranch: "master");
            await target.Load(CreatePullRequest());

            Assert.Equal(CheckoutMode.Fetch, target.CheckoutMode);
            Assert.True(target.Checkout.CanExecute(null));
        }

        [Fact]
        public async Task CheckoutDisabledMessageShouldBeSetWhenNeedsFetchAndWorkingDirectoryDirty()
        {
            var target = CreateTarget(currentBranch: "master", dirty: true);
            await target.Load(CreatePullRequest());

            Assert.Equal(CheckoutMode.Fetch, target.CheckoutMode);
            Assert.Equal("Cannot checkout pull request as your working directory has uncommitted changes.", target.CheckoutDisabledMessage);
            Assert.False(target.Checkout.CanExecute(null));
        }

        [Fact]
        public async Task CheckoutModeShouldBePush()
        {
            var target = CreateTarget(
                currentBranch: "pr/123",
                existingPrBranch: "pr/123",
                prFromFork: false,
                aheadBy: 1);
            await target.Load(CreatePullRequest());

            Assert.Equal(CheckoutMode.Push, target.CheckoutMode);
            Assert.False(target.Checkout.CanExecute(null));
        }

        [Fact]
        public async Task CheckoutModeShouldBeInvalidStateWhenBranchFromForkHasLocalCommits()
        {
            var target = CreateTarget(
                currentBranch: "pr/123",
                existingPrBranch: "pr/123",
                prFromFork: true,
                aheadBy: 1,
                behindBy: 2);
            await target.Load(CreatePullRequest());

            Assert.Equal(CheckoutMode.InvalidState, target.CheckoutMode);
            Assert.True(target.Checkout.CanExecute(null));
        }

        [Fact]
        public async Task CheckoutDisabledMessageShouldBeSetWhenInvalidStateAndWorkingDirectoryDirty()
        {
            var target = CreateTarget(
                currentBranch: "pr/123",
                existingPrBranch: "pr/123",
                prFromFork: true,
                aheadBy: 1,
                behindBy: 2,
                dirty: true);
            await target.Load(CreatePullRequest());

            Assert.Equal(CheckoutMode.InvalidState, target.CheckoutMode);
            Assert.Equal("Cannot checkout pull request as your working directory has uncommitted changes.", target.CheckoutDisabledMessage);
            Assert.False(target.Checkout.CanExecute(null));
        }

        [Fact]
        public async Task CheckoutNeedsPullShouldCallService()
        {
            var target = CreateTargetAndService(
                currentBranch: "pr/123",
                existingPrBranch: "pr/123",
                behindBy: 3);

            await target.Item1.Load(CreatePullRequest());

            Assert.Equal(CheckoutMode.NeedsPull, target.Item1.CheckoutMode);

            target.Item1.Checkout.Execute(null);

            var unused = target.Item2.Received().Pull(Arg.Any<ILocalRepositoryModel>());
        }

        [Fact]
        public async Task CheckoutSwitchShouldCallService()
        {
            var target = CreateTargetAndService(
                currentBranch: "master",
                existingPrBranch: "pr/123");
            var pr = CreatePullRequest();

            await target.Item1.Load(pr);

            Assert.Equal(CheckoutMode.Switch, target.Item1.CheckoutMode);

            target.Item1.Checkout.Execute(null);

            var unused = target.Item2.Received().SwitchToBranch(Arg.Any<ILocalRepositoryModel>(), pr);
        }

        [Fact]
        public async Task CheckoutFetchShouldCallService()
        {
            var target = CreateTargetAndService(currentBranch: "master");
            target.Item2.GetDefaultLocalBranchName(Arg.Any<ILocalRepositoryModel>(), 1, Arg.Any<string>())
                .Returns(Observable.Return("pr/1-foo"));

            await target.Item1.Load(CreatePullRequest());

            Assert.Equal(CheckoutMode.Fetch, target.Item1.CheckoutMode);

            target.Item1.Checkout.Execute(null);

            var unused = target.Item2.Received().FetchAndCheckout(Arg.Any<ILocalRepositoryModel>(), target.Item1.Model.Number, "pr/1-foo");
        }

        [Fact]
        public async Task CheckoutInvalidStateShouldCallService()
        {
            var target = CreateTargetAndService(
                currentBranch: "pr/123",
                existingPrBranch: "pr/123",
                aheadBy: 1);

            target.Item2.IsPullRequestFromFork(
                Arg.Any<ILocalRepositoryModel>(),
                Arg.Any<IPullRequestModel>()).Returns(true);
            target.Item2.GetDefaultLocalBranchName(Arg.Any<ILocalRepositoryModel>(), 1, Arg.Any<string>())
                .Returns(Observable.Return("pr/1-foo"));

            await target.Item1.Load(CreatePullRequest());

            Assert.Equal(CheckoutMode.InvalidState, target.Item1.CheckoutMode);

            target.Item1.Checkout.Execute(null);

            var unused = target.Item2.Received().UnmarkLocalBranch(Arg.Any<ILocalRepositoryModel>());
            unused = target.Item2.Received().FetchAndCheckout(Arg.Any<ILocalRepositoryModel>(), target.Item1.Model.Number, "pr/1-foo");
        }

        [Fact]
        public async Task ShouldAcceptNullHead()
        {
            var target = CreateTarget();
            var model = CreatePullRequest();

            // PullRequest.Head can be null for example if a user deletes the repository after creating the PR.
            model.Head = null;

            await target.Load(model);

            Assert.Equal("[Invalid]", target.SourceBranchDisplayName);
        }

        PullRequestDetailViewModel CreateTarget(
            string currentBranch = "master",
            string existingPrBranch = null,
            bool prFromFork = false,
            bool dirty = false,
            int aheadBy = 0,
            int behindBy = 0)
        {
            return CreateTargetAndService(
                currentBranch: currentBranch,
                existingPrBranch: existingPrBranch,
                prFromFork: prFromFork,
                dirty: dirty,
                aheadBy: aheadBy,
                behindBy: behindBy).Item1;
        }

        Tuple<PullRequestDetailViewModel, IPullRequestService> CreateTargetAndService(
            string currentBranch = "master",
            string existingPrBranch = null,
            bool prFromFork = false,
            bool dirty = false,
            int aheadBy = 0,
            int behindBy = 0)
        {
            var repository = Substitute.For<ILocalRepositoryModel>();
            var currentBranchModel = new BranchModel(currentBranch, repository);
            repository.CurrentBranch.Returns(currentBranchModel);
            repository.CloneUrl.Returns(new UriString(Uri.ToString()));

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

            pullRequestService.IsPullRequestFromFork(repository, Arg.Any<IPullRequestModel>()).Returns(prFromFork);
            pullRequestService.IsCleanForCheckout(repository).Returns(Observable.Return(!dirty));

            var divergence = Substitute.For<HistoryDivergence>();
            divergence.AheadBy.Returns(aheadBy);
            divergence.BehindBy.Returns(behindBy);
            pullRequestService.CalculateHistoryDivergence(repository, Arg.Any<int>())
                .Returns(Observable.Return(divergence));

            var vm = new PullRequestDetailViewModel(
                repository,
                Substitute.For<IModelService>(),
                pullRequestService,
                Substitute.For<IPackageSettings>());

            return Tuple.Create(vm, pullRequestService);
        }

        PullRequestModel CreatePullRequest(string body = "PR Body")
        {
            var author = Substitute.For<IAccount>();

            return new PullRequestModel(1, "PR 1", author, DateTimeOffset.Now)
            {
                State = PullRequestStateEnum.Open,
                Body = string.Empty,
                Head = new GitReferenceModel("source", "foo:baz", "sha", "https://github.com/foo/bar.git"),
                Base = new GitReferenceModel("dest", "foo:bar", "sha", "https://github.com/foo/bar.git"),
            };
        }
    }
}
