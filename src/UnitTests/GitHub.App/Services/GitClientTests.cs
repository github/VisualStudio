using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Services;
using LibGit2Sharp;
using NSubstitute;
using Xunit;

public class GitClientTests
{
    public class TheIsHeadPushedMethod : TestBaseClass
    {
        [Theory]
        [InlineData(true, "sha", "sha", true)]
        [InlineData(true, "xxx", "yyy", false)]
        [InlineData(true, "headSha", null, false)]
        [InlineData(false, "sha", "sha", false)]
        public async Task IsHeadPushed(bool istracking, string headSha, string trackedBranchSha, bool expectHeadPushed)
        {
            var gitClient = new GitClient(Substitute.For<IGitHubCredentialProvider>());
            var repository = MockTrackedBranchRepository(istracking, headSha, trackedBranchSha);

            var isHeadPushed = await gitClient.IsHeadPushed(repository);

            Assert.Equal(expectHeadPushed, isHeadPushed);
        }

        static IRepository MockTrackedBranchRepository(bool isTracking, string headSha, string trackedBranchSha)
        {
            var trackedBranch = Substitute.For<Branch>();
            var trackedBranchTip = MockCommitOrNull(trackedBranchSha);
            trackedBranch.Tip.Returns(trackedBranchTip);
            var headBranch = Substitute.For<Branch>();
            var headTip = MockCommitOrNull(headSha);
            headBranch.Tip.Returns(headTip);
            headBranch.IsTracking.Returns(isTracking);
            headBranch.TrackedBranch.Returns(trackedBranch);
            var repository = Substitute.For<IRepository>();
            repository.Head.Returns(headBranch);
            return repository;
        }

        static Commit MockCommitOrNull(string sha)
        {
            if (sha != null)
            {
                var commit = Substitute.For<Commit>();
                commit.Sha.Returns(sha);
                return commit;
            }

            return null;
        }
    }

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

    public class TheGetPullRequestMergeBaseMethod : TestBaseClass
    {
        [Fact]
        public async Task LocalBaseHeadAndMergeBase_DontFetch()
        {
            var remoteName = "origin";
            var baseSha = "baseSha";
            var headSha = "headSha";
            var expectMergeBaseSha = "mergeBaseSha";
            var baseRef = "master";
            var pullNumber = 666;
            var repo = MockRepo(baseSha, headSha, expectMergeBaseSha);
            var gitClient = new GitClient(Substitute.For<IGitHubCredentialProvider>());

            var mergeBaseSha = await gitClient.GetPullRequestMergeBase(repo, remoteName, baseSha, headSha, baseRef, pullNumber);

            repo.Network.DidNotReceiveWithAnyArgs().Fetch(null as Remote, null, null as FetchOptions);
            Assert.Equal(mergeBaseSha, expectMergeBaseSha);
        }

        [Theory]
        [InlineData("baseSha", "headSha", "mergeBaseSha", 0)]
        [InlineData(null, "headSha", "mergeBaseSha", 1)]
        [InlineData("baseSha", null, "mergeBaseSha", 1)]
        [InlineData("baseSha", "headSha", null, 1)]
        public async Task WhenToFetch(string baseSha, string headSha, string mergeBaseSha, int receivedFetch)
        {
            var remoteName = "origin";
            var baseRef = "master";
            var pullNumber = 666;
            var repo = MockRepo(baseSha, headSha, mergeBaseSha);
            var gitClient = new GitClient(Substitute.For<IGitHubCredentialProvider>());

            await gitClient.GetPullRequestMergeBase(repo, remoteName, baseSha, headSha, baseRef, pullNumber);

            repo.Network.Received(receivedFetch).Fetch(Arg.Any<Remote>(), Arg.Any<string[]>(), Arg.Any<FetchOptions>());
        }

        [Theory]
        [InlineData("baseSha", null, "mergeBaseSha", "master", 666, "master")]
        [InlineData("baseSha", null, "mergeBaseSha", "master", 666, "refs/pull/666/head")]
        public async Task WhatToFetch(string baseSha, string headSha, string mergeBaseSha, string baseRef, int pullNumber,
            string expectRefSpec)
        {
            var remoteName = "origin";
            var repo = MockRepo(baseSha, headSha, mergeBaseSha);
            var gitClient = new GitClient(Substitute.For<IGitHubCredentialProvider>());

            await gitClient.GetPullRequestMergeBase(repo, remoteName, baseSha, headSha, baseRef, pullNumber);

            repo.Network.Received(1).Fetch(Arg.Any<Remote>(), Arg.Is<string[]>(x => x.Contains(expectRefSpec)), Arg.Any<FetchOptions>());
        }

        static IRepository MockRepo(string baseSha, string headSha, string mergeBaseSha)
        {
            var repo = Substitute.For<IRepository>();
            var baseCommit = Substitute.For<Commit>();
            var headCommit = Substitute.For<Commit>();
            var mergeBaseCommit = Substitute.For<Commit>();
            mergeBaseCommit.Sha.Returns(mergeBaseSha);

            if (baseSha != null)
            {
                repo.Lookup<Commit>(baseSha).Returns(baseCommit);
            }

            if (headSha != null)
            {
                repo.Lookup<Commit>(headSha).Returns(headCommit);
            }

            repo.ObjectDatabase.FindMergeBase(baseCommit, headCommit).Returns(mergeBaseCommit);
            return repo;
        }
    }
}
