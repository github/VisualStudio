using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitHub.App.Services;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using NSubstitute;
using NUnit.Framework;

public class LocalRepositoriesTests : TestBaseClass
{
    const string GitHubAddress = "https://github.com";

    [Test]
    public void RepositoriesShouldInitiallyBeEmpty()
    {
        var service = CreateVSGitServices("repo1", "repo2");
        var target = new LocalRepositories(service);

        Assert.That(target.Repositories, Is.Empty);
    }

    [Test]
    public async Task RefreshShouldLoadRepositories()
    {
        var service = CreateVSGitServices("repo1", "repo2");
        var target = new LocalRepositories(service);

        await target.Refresh();

        Assert.That(
            new[] { "repo1", "repo2" },
            Is.EqualTo(target.Repositories.Select(x => x.Name).ToList()));
    }

    [Test]
    public async Task RefreshShouldAddNewRepository()
    {
        var service = CreateVSGitServices("repo1", "repo2");
        var target = new LocalRepositories(service);

        await target.Refresh();

        Assert.That(2, Is.EqualTo(target.Repositories.Count));

        var existing = service.GetKnownRepositories();
        var newRepo = CreateRepository("new");
        service.GetKnownRepositories().Returns(existing.Concat(new[] { newRepo }));

        await target.Refresh();

        Assert.That(
            new[] { "repo1", "repo2", "new" },
            Is.EqualTo(target.Repositories.Select(x => x.Name).ToList()));
    }

    [Test]
    public async Task RefreshShouldRemoveRepository()
    {
        var service = CreateVSGitServices("repo1", "repo2");
        var target = new LocalRepositories(service);

        await target.Refresh();

        Assert.That(2, Is.EqualTo(target.Repositories.Count));

        var existing = service.GetKnownRepositories();
        service.GetKnownRepositories().Returns(existing.Skip(1).Take(1));

        await target.Refresh();

        Assert.That(
            new[] { "repo2" },
            Is.EqualTo(target.Repositories.Select(x => x.Name).ToList()));
    }

    [Test]
    public async Task GetRepositoriesForAddressShouldFilterRepositories()
    {
        var service = CreateVSGitServices(
            Tuple.Create("repo1", GitHubAddress),
            Tuple.Create("repo2", GitHubAddress),
            Tuple.Create("repo2", "https://another.com"));
        var target = new LocalRepositories(service);

        await target.Refresh();

        Assert.That(3, Is.EqualTo(target.Repositories.Count));

        var result = target.GetRepositoriesForAddress(HostAddress.Create(GitHubAddress));

        Assert.That(2, Is.EqualTo(result.Count));
    }

    [Test]
    public async Task GetRepositoriesForAddressShouldSortRepositories()
    {
        var service = CreateVSGitServices("c", "a", "b");
        var target = new LocalRepositories(service);

        await target.Refresh();
        var result = target.GetRepositoriesForAddress(HostAddress.Create(GitHubAddress));

        Assert.That(
            new[] { "a", "b", "c" },
            Is.EqualTo(result.Select(x => x.Name).ToList()));
    }

    static IVSGitServices CreateVSGitServices(params string[] names)
    {
        return CreateVSGitServices(names.Select(x => Tuple.Create(x, GitHubAddress)).ToArray());
    }

    static IVSGitServices CreateVSGitServices(params Tuple<string, string>[] namesAndAddresses)
    {
        var result = Substitute.For<IVSGitServices>();
        var repositories = new List<LocalRepositoryModel>(namesAndAddresses.Select(CreateRepository));
        result.GetKnownRepositories().Returns(repositories);
        return result;
    }

    static LocalRepositoryModel CreateRepository(string name)
    {
        return CreateRepository(Tuple.Create(name, "https://github.com"));
    }

    static LocalRepositoryModel CreateRepository(Tuple<string, string> nameAndAddress)
    {
        return new LocalRepositoryModel
        {
            Name = nameAndAddress.Item1,
            CloneUrl = new UriString(nameAndAddress.Item2)
        };
    }
}
