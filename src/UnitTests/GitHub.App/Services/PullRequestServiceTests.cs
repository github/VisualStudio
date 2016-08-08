using System.Reactive.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using UnitTests;
using GitHub.Models;
using System;
using GitHub.Services;

public class PullRequestServiceTests : TestBaseClass
{
    [Fact]
    public async Task CreatePullRequestAllArgsMandatory()
    {
        var serviceProvider = Substitutes.ServiceProvider;
        var service = new PullRequestService();

        IRepositoryHost host = null;
        ISimpleRepositoryModel repository = null;
        string title = null;
        string body = null;
        IBranch source = null;
        IBranch target = null;

        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(host, repository, title, body, source, target));

        host = serviceProvider.GetRepositoryHosts().GitHubHost;
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(host, repository, title, body, source, target));

        repository = new SimpleRepositoryModel("name", new GitHub.Primitives.UriString("http://github.com/github/stuff"));
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(host, repository, title, body, source, target));

        title = "a title";
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(host, repository, title, body, source, target));

        body = "a body";
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(host, repository, title, body, source, target));

        source = new BranchModel() { Name = "source" };
        Assert.Throws<ArgumentNullException>(() => service.CreatePullRequest(host, repository, title, body, source, target));

        target = new BranchModel() { Name = "target" };
        var pr = await service.CreatePullRequest(host, repository, title, body, source, target);

        Assert.NotNull(pr);
    }

}
