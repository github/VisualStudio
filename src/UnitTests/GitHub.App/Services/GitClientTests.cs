using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Services;
using GitHub.VisualStudio.UI;
using LibGit2Sharp;
using NSubstitute;
using Xunit;

public class GitClientTests
{
    public class ThePushMethod : TestBaseClass
    {
        [Fact]
        public async Task PushesToDefaultOrigin()
        {
            var origin = Substitute.For<Remote>();
            var head = Substitute.For<Branch>();
            head.Commits.Returns(new FakeCommitLog { Substitute.For<Commit>() });
            var repository = Substitute.For<IRepository>();
            repository.Head.Returns(head);
            repository.Network.Remotes["origin"].Returns(origin);
            var gitClient = new GitClient(Substitute.For<IGitHubCredentialProvider>());

            await gitClient.Push(repository, "master", "origin");

            repository.Network.Received().Push(origin,"HEAD", @"refs/heads/master", Arg.Any<PushOptions>());
        }

        [Fact]
        public async Task DoesNotPushEmptyRepository()
        {
            var repository = Substitute.For<IRepository>();
            var gitClient = new GitClient(Substitute.For<IGitHubCredentialProvider>());

            await gitClient.Push(repository, "master", "origin");

            repository.Network.DidNotReceive()
                .Push(Args.LibgGit2Remote, Args.String, Args.String);
        }
    }

    public class TheSetRemoteMethod : TestBaseClass
    {
        [Fact]
        public async Task SetsTheConfigToTheRemoteBranch()
        {
            var config = Substitute.For<Configuration>();
            var repository = Substitute.For<IRepository>();
            repository.Config.Returns(config);
            var gitClient = new GitClient(Substitute.For<IGitHubCredentialProvider>());

            await gitClient.SetRemote(repository, "origin", new Uri("https://github.com/foo/bar"));

            config.Received().Set<string>("remote.origin.url", "https://github.com/foo/bar");
            config.Received().Set<string>("remote.origin.fetch", "+refs/heads/*:refs/remotes/origin/*");
        }
    }

    public class TheSetTrackingMethod : TestBaseClass
    {
        [Fact]
        public async Task SetsTheRemoteTrackingBranch()
        {
            var config = Substitute.For<Configuration>();
            var origin = Substitute.For<Remote>();
            var branches = Substitute.For<BranchCollection>();
            var repository = Substitute.For<IRepository>();
            repository.Config.Returns(config);
            repository.Branches.Returns(branches);
            repository.Network.Remotes["origin"].Returns(origin);
            var localBranch = Substitute.For<Branch>();
            var remoteBranch = Substitute.For<Branch>(); ;
            branches["refs/heads/master"].Returns(localBranch);
            branches["refs/remotes/origin/master"].Returns(remoteBranch);

            var gitClient = new GitClient(Substitute.For<IGitHubCredentialProvider>());

            await gitClient.SetTrackingBranch(repository, "master", "origin");

            branches.Received().Update(localBranch, Arg.Any<Action<BranchUpdater>>());
        }
    }

    public class TheGetTrackingBranchMethod : TestBaseClass
    {
        public static readonly byte[] RepositoryBinary = UnitTests.Properties.Resources.test_get_tracking_branch;
        public const string RepositoryBinaryName = nameof(UnitTests.Properties.Resources.test_get_tracking_branch);

        [Fact]
        public async void FindsUpToDateTrackingBranch()
        {
            using (var temp = new TempRepository(RepositoryBinaryName, RepositoryBinary))
            {
                var target = new GitClient(Substitute.For<IGitHubCredentialProvider>());
                var result = await target.GetTrackingBranch(temp.Repository, "refs/pull/1/head").DefaultIfEmpty();

                Assert.NotNull(result);
                Assert.Equal("refs/heads/pr-1", result.CanonicalName);
            }
        }

        [Fact]
        public async void FindsNotUpToDateTrackingBranch()
        {
            using (var temp = new TempRepository(RepositoryBinaryName, RepositoryBinary))
            {
                var target = new GitClient(Substitute.For<IGitHubCredentialProvider>());
                var result = await target.GetTrackingBranch(temp.Repository, "refs/pull/2/head").DefaultIfEmpty();

                Assert.NotNull(result);
                Assert.Equal("refs/heads/pr-2", result.CanonicalName);
            }
        }
    }
}
