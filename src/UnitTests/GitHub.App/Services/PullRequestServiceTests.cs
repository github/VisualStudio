using System.Reactive.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using UnitTests;
using GitHub.Models;
using System;
using GitHub.Services;
using Rothko;
using LibGit2Sharp;
using System.Collections.Generic;

public class PullRequestServiceTests : TestBaseClass
{
    [Fact]
    public void CreatePullRequestAllArgsMandatory()
    {
        var serviceProvider = Substitutes.ServiceProvider;
        var service = new PullRequestService(Substitute.For<IGitClient>(), serviceProvider.GetGitService(), serviceProvider.GetOperatingSystem(), Substitute.For<IUsageTracker>());

        IRepositoryHost host = null;
        ILocalRepositoryModel sourceRepo = null;
        ILocalRepositoryModel targetRepo = null;
        string title = null;
        string body = null;
        IBranch source = null;
        IBranch target = null;

        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(host, sourceRepo, targetRepo, source, target, title, body));

        host = serviceProvider.GetRepositoryHosts().GitHubHost;
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(host, sourceRepo, targetRepo, source, target, title, body));

        sourceRepo = new LocalRepositoryModel("name", new GitHub.Primitives.UriString("http://github.com/github/stuff"), "c:\\path");
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(host, sourceRepo, targetRepo, source, target, title, body));

        targetRepo = new LocalRepositoryModel("name", new GitHub.Primitives.UriString("http://github.com/github/stuff"), "c:\\path");
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(host, sourceRepo, targetRepo, source, target, title, body));

        title = "a title";
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(host, sourceRepo, targetRepo, source, target, title, body));

        body = "a body";
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(host, sourceRepo, targetRepo, source, target, title, body));

        source = new BranchModel("source", sourceRepo);
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(host, sourceRepo, targetRepo, source, target, title, body));

        target = new BranchModel("target", targetRepo);
        var pr = service.CreatePullRequest(host, sourceRepo, targetRepo, source, target, title, body);

        Assert.NotNull(pr);
    }

    public class TheGetDefaultLocalBranchNameMethod
    {
        [Fact]
        public void ShouldReturnCorrectDefaultLocalBranchName()
        {
            var service = new PullRequestService(
                Substitute.For<IGitClient>(),
                MockGitService(),
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IUsageTracker>());

            var localRepo = Substitute.For<ILocalRepositoryModel>();
            var result = service.GetDefaultLocalBranchName(localRepo, 123, "Pull requests can be \"named\" all sorts of thing's (sic)");
            Assert.Equal("pr/123-pull-requests-can-be-named-all-sorts-of-thing-s-sic-", result);
        }

        [Fact]
        public void DefaultLocalBranchNameShouldNotClashWithExistingBranchNames()
        {
            var service = new PullRequestService(
                Substitute.For<IGitClient>(),
                MockGitService(),
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IUsageTracker>());

            var localRepo = Substitute.For<ILocalRepositoryModel>();
            var result = service.GetDefaultLocalBranchName(localRepo, 123, "foo1");
            Assert.Equal("pr/123-foo1-3", result);
        }

        static IGitService MockGitService()
        {
            var repository = Substitute.For<IRepository>();
            var branches = MockBranches("pr/123-foo1", "pr/123-foo1-2");
            repository.Branches.Returns(branches);

            var result = Substitute.For<IGitService>();
            result.GetRepository(Arg.Any<string>()).Returns(repository);
            return result;
        }
    }

    static BranchCollection MockBranches(params string[] names)
    {
        var result = Substitute.For<BranchCollection>();

        foreach (var name in names)
        {
            var branch = Substitute.For<Branch>();
            branch.CanonicalName.Returns("refs/heads/" + name);
            result[name].Returns(branch);
        }

        return result;
    }
}
