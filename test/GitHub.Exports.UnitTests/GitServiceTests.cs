using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Services;
using LibGit2Sharp;
using NSubstitute;
using NUnit.Framework;

public class GitServiceTests
{
    [TestCase("asdf", null)]
    [TestCase("", null)]
    [TestCase(null, null)]
    [TestCase("file:///C:/dev/exp/foo", "file:///C:/dev/exp/foo")]
    [TestCase("http://example.com/", "http://example.com/")]
    [TestCase("http://haacked@example.com/foo/bar", "http://example.com/foo/bar")]
    [TestCase("https://github.com/github/Windows", "https://github.com/github/Windows")]
    [TestCase("https://github.com/github/Windows.git", "https://github.com/github/Windows")]
    [TestCase("https://haacked@github.com/github/Windows.git", "https://github.com/github/Windows")]
    [TestCase("http://example.com:4000/github/Windows", "http://example.com:4000/github/Windows")]
    [TestCase("git@192.168.1.2:github/Windows.git", "https://192.168.1.2/github/Windows")]
    [TestCase("git@example.com:org/repo.git", "https://example.com/org/repo")]
    [TestCase("ssh://git@github.com:443/shana/cef", "https://github.com/shana/cef")]
    [TestCase("ssh://git@example.com:23/haacked/encourage", "https://example.com:23/haacked/encourage")]
    public void GetUriShouldNotThrow(string url, string expected)
    {
        var origin = Substitute.For<Remote>();
        origin.Url.Returns(url);
        var repository = Substitute.For<IRepository>();
        repository.Network.Remotes["origin"].Returns(origin);
        var repositoryFacade = new RepositoryFacade();
        var target = new GitService(repositoryFacade);

        var repositoryUrl = target.GetUri(repository)?.ToString();

        Assert.That(expected, Is.EqualTo(repositoryUrl));
    }

    public class TheGetLatestPushedShaMethod
    {
        [TestCase("777", "777", "777")]
        [TestCase("666", "777", null)]
        public async Task Return_Sha_When_Local_And_Remote_Shas_Match(string localSha, string remoteSha, string expectSha)
        {
            var path = "path";
            var trackedBranch = CreateBranch(remoteSha);
            var headBranch = CreateBranch(localSha, trackedBranch);
            var repo = CreateRepository(headBranch);
            var repositoryFacade = CreateRepositoryFacade(path, repo);
            var target = new GitService(repositoryFacade);

            var sha = await target.GetLatestPushedSha(path).ConfigureAwait(false);

            Assert.That(sha, Is.EqualTo(expectSha));
        }

        static IRepositoryFacade CreateRepositoryFacade(string path, IRepository repo)
        {
            var repositoryFacade = Substitute.For<IRepositoryFacade>();
            repositoryFacade.Discover(path).Returns(path);
            repositoryFacade.NewRepository(path).Returns(repo);
            return repositoryFacade;
        }

        static IRepository CreateRepository(Branch headBranch)
        {
            var repo = Substitute.For<IRepository>();
            repo.Head.Returns(headBranch);
            var branchCollection = Substitute.For<BranchCollection>();
            var branches = new List<Branch> { headBranch };
            branchCollection.GetEnumerator().Returns(_ => branches.GetEnumerator());
            repo.Branches.Returns(branchCollection);
            return repo;
        }

        static Branch CreateBranch(string tipSha, Branch trackedBranch = null)
        {
            var tipCommit = Substitute.For<Commit>();
            tipCommit.Sha.Returns(tipSha);
            var branch = Substitute.For<Branch>();
            var commitLog = Substitute.For<ICommitLog>();
            var commits = new List<Commit> { tipCommit };
            commitLog.GetEnumerator().Returns(_ => commits.GetEnumerator());
            branch.Commits.Returns(commitLog);
            branch.Tip.Returns(tipCommit);
            if (trackedBranch != null)
            {
                var trackingDetails = Substitute.For<BranchTrackingDetails>();
                trackingDetails.CommonAncestor.Returns(tipCommit);
                branch.IsTracking.Returns(true);
                branch.TrackedBranch.Returns(trackedBranch);
                branch.TrackingDetails.Returns(trackingDetails);
            }

            return branch;
        }
    }
}
