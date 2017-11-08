using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitHub.App.Services;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using NSubstitute;
using Xunit;

public class LocalRepositoriesTests : TestBaseClass
{
    const string GitHubAddress = "https://github.com";

    [Fact]
    public void RepositoriesShouldInitiallyBeEmpty()
    {
        var service = CreateVSGitServices("repo1", "repo2");
        var target = new LocalRepositories(service);

        Assert.Empty(target.Repositories);
    }

    [Fact]
    public async Task RefreshShouldLoadRepositories()
    {
        var service = CreateVSGitServices("repo1", "repo2");
        var target = new LocalRepositories(service);

        await target.Refresh();

        Assert.Equal(
            new[] { "repo1", "repo2" },
            target.Repositories.Select(x => x.Name).ToList());
    }

    [Fact]
    public async Task RefreshShouldAddNewRepository()
    {
        var service = CreateVSGitServices("repo1", "repo2");
        var target = new LocalRepositories(service);

        await target.Refresh();

        Assert.Equal(2, target.Repositories.Count);

        var existing = service.GetKnownRepositories();
        var newRepo = CreateRepository("new");
        service.GetKnownRepositories().Returns(existing.Concat(new[] { newRepo }));

        await target.Refresh();

        Assert.Equal(
            new[] { "repo1", "repo2", "new" },
            target.Repositories.Select(x => x.Name).ToList());
    }

    [Fact]
    public async Task RefreshShouldRemoveRepository()
    {
        var service = CreateVSGitServices("repo1", "repo2");
        var target = new LocalRepositories(service);

        await target.Refresh();

        Assert.Equal(2, target.Repositories.Count);

        var existing = service.GetKnownRepositories();
        service.GetKnownRepositories().Returns(existing.Skip(1).Take(1));

        await target.Refresh();

        Assert.Equal(
            new[] { "repo2" },
            target.Repositories.Select(x => x.Name).ToList());
    }

    [Fact]
    public async Task GetRepositoriesForAddressShouldFilterRepositories()
    {
        var service = CreateVSGitServices(
            Tuple.Create("repo1", GitHubAddress),
            Tuple.Create("repo2", GitHubAddress),
            Tuple.Create("repo2", "https://another.com"));
        var target = new LocalRepositories(service);

        await target.Refresh();

        Assert.Equal(3, target.Repositories.Count);

        var result = target.GetRepositoriesForAddress(HostAddress.Create(GitHubAddress));

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetRepositoriesForAddressShouldSortRepositories()
    {
        var service = CreateVSGitServices("c", "a", "b");
        var target = new LocalRepositories(service);

        await target.Refresh();
        var result = target.GetRepositoriesForAddress(HostAddress.Create(GitHubAddress));

        Assert.Equal(
            new[] { "a", "b", "c" },
            result.Select(x => x.Name).ToList());
    }

    static IVSGitServices CreateVSGitServices(params string[] names)
    {
        return CreateVSGitServices(names.Select(x => Tuple.Create(x, GitHubAddress)).ToArray());
    }

    static IVSGitServices CreateVSGitServices(params Tuple<string, string>[] namesAndAddresses)
    {
        var result = Substitute.For<IVSGitServices>();
        var repositories = new List<ILocalRepositoryModel>(namesAndAddresses.Select(CreateRepository));
        result.GetKnownRepositories().Returns(repositories);
        return result;
    }

    static ILocalRepositoryModel CreateRepository(string name)
    {
        return CreateRepository(Tuple.Create(name, "https://github.com"));
    }

    static ILocalRepositoryModel CreateRepository(Tuple<string, string> nameAndAddress)
    {
        var result = Substitute.For<ILocalRepositoryModel>();
        result.Name.Returns(nameAndAddress.Item1);
        result.CloneUrl.Returns(new UriString(nameAndAddress.Item2));
        return result;
    }
}
