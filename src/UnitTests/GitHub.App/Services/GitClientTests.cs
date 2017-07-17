using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Services;
using LibGit2Sharp;
using NSubstitute;
using Xunit;
using GitHub.Primitives;
using System.Collections.Generic;

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

    public class TheGetPullRequestMergeBaseMethod : TestBaseClass
    {
        [Fact]
        public async Task LocalBaseHeadAndMergeBase_DontFetch()
        {
            var baseUrl = new UriString("https://github.com/owner/repo");
            var headUrl = new UriString("https://github.com/owner/repo");
            var baseSha = "baseSha";
            var headSha = "headSha";
            var expectMergeBaseSha = "mergeBaseSha";
            var baseRef = "master";
            var headRef = "headRef";
            var repo = MockRepo(baseSha, headSha, expectMergeBaseSha);
            var gitClient = new GitClient(Substitute.For<IGitHubCredentialProvider>());

            var mergeBaseSha = await gitClient.GetPullRequestMergeBase(repo, baseUrl, headUrl, baseSha, headSha, baseRef, headRef);

            repo.Network.DidNotReceiveWithAnyArgs().Fetch(null as Remote, null, null as FetchOptions);
            Assert.Equal(expectMergeBaseSha, mergeBaseSha);
        }

        [Theory]
        [InlineData("https://github.com/owner/repo", "https://github.com/owner/repo", "baseSha", "headSha", "mergeBaseSha", 0)]
        [InlineData("https://github.com/owner/repo", "https://github.com/owner/repo", null, "headSha", "mergeBaseSha", 1)]
        [InlineData("https://github.com/owner/repo", "https://github.com/owner/repo", "baseSha", null, "mergeBaseSha", 1)]
        [InlineData("https://github.com/owner/repo", "https://github.com/owner/repo", "baseSha", "headSha", null, 0)]
        public async Task WhenToFetch(string baseUrl, string headUrl, string baseSha, string headSha, string mergeBaseSha, int receivedFetch)
        {
            var baseUri = new UriString(baseUrl);
            var headUri = new UriString(headUrl);
            var baseRef = "master";
            var headRef = "headRef";
            var repo = MockRepo(baseSha, headSha, mergeBaseSha);
            var remote = Substitute.For<Remote>();
            repo.Network.Remotes.Add(null, null).ReturnsForAnyArgs(remote);
            var gitClient = new GitClient(Substitute.For<IGitHubCredentialProvider>());

            await gitClient.GetPullRequestMergeBase(repo, baseUri, headUri, baseSha, headSha, baseRef, headRef);

            repo.Network.Received(receivedFetch).Fetch(Arg.Any<Remote>(), Arg.Any<string[]>(), Arg.Any<FetchOptions>());
        }

        [Theory]
        [InlineData("baseSha", null, "mergeBaseSha", "baseRef", "headRef", "headRef")]
        [InlineData(null, "headSha", "mergeBaseSha", "baseRef", "headRef", "baseRef")]
        public async Task WhatToFetch(string baseSha, string headSha, string mergeBaseSha, string baseRef, string headRef,
            string expectRefSpec)
        {
            var repo = MockRepo(baseSha, headSha, mergeBaseSha);
            var baseUrl = new UriString("https://github.com/owner/repo");
            var headUrl = new UriString("https://github.com/owner/repo");
            var gitClient = new GitClient(Substitute.For<IGitHubCredentialProvider>());

            await gitClient.GetPullRequestMergeBase(repo, baseUrl, headUrl, baseSha, headSha, baseRef, headRef);

            repo.Network.Received(1).Fetch(Arg.Any<Remote>(), Arg.Is<IEnumerable<string>>(x => x.Contains(expectRefSpec)), Arg.Any<FetchOptions>());
        }

        static IRepository MockRepo(string baseSha, string headSha, string mergeBaseSha)
        {
            var repo = Substitute.For<IRepository>();
            var baseCommit = Substitute.For<Commit>();
            var headCommit = Substitute.For<Commit>();
            var mergeBaseCommit = Substitute.For<Commit>();
            mergeBaseCommit.Sha.Returns(mergeBaseSha);

            if(baseSha != null)
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
