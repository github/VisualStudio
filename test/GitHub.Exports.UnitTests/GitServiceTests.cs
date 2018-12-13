using GitHub.Services;
using NSubstitute;
using NUnit.Framework;
using IRepository = LibGit2Sharp.IRepository;
using Remote = LibGit2Sharp.Remote;

public class GitServiceTests
{
    public class CreateLocalRepositoryModelTests : TestBaseClass
    {
        [Test]
        public void NoRemoteUrl()
        {
            using (var temp = new TempDirectory())
            {
                var repositoryFacade = Substitute.For<IRepositoryFacade>();
                var gitService = new GitService(repositoryFacade);
                var path = temp.Directory.CreateSubdirectory("repo-name");

                var model = gitService.CreateLocalRepositoryModel(path.FullName);

                Assert.That(model.Name, Is.EqualTo("repo-name"));
            }
        }

        [Test]
        public void WithRemoteUrl()
        {
            using (var temp = new TempDirectory())
            {
                var path = temp.Directory.CreateSubdirectory("repo-name");
                var repository = CreateRepositoryWithOrigin("https://github.com/user/repo-name");
                var repositoryFacade = CreateRepositoryFacade(path.FullName, repository);
                var gitService = new GitService(repositoryFacade);

                var model = gitService.CreateLocalRepositoryModel(path.FullName);

                Assert.That(model.Name, Is.EqualTo("repo-name"));
                Assert.That(model.Owner, Is.EqualTo("user"));
            }
        }
    }

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
        var path = "path";
        var repository = CreateRepository();
        repository.Network.Remotes["origin"].Returns(origin);
        var repositoryFacade = CreateRepositoryFacade(path, repository);
        var target = new GitService(repositoryFacade);

        var repositoryUrl = target.GetUri(repository)?.ToString();

        Assert.That(expected, Is.EqualTo(repositoryUrl));
    }

    static IRepositoryFacade CreateRepositoryFacade(string path, IRepository repo)
    {
        var repositoryFacade = Substitute.For<IRepositoryFacade>();
        repositoryFacade.Discover(path).Returns(path);
        repositoryFacade.NewRepository(path).Returns(repo);
        return repositoryFacade;
    }

    static IRepository CreateRepositoryWithOrigin(string originUrl)
    {
        var repo = CreateRepository();
        var origin = Substitute.For<Remote>();
        origin.Url.Returns(originUrl);
        repo.Network.Remotes["origin"].Returns(origin);
        return repo;
    }

    static IRepository CreateRepository()
    {
        return Substitute.For<IRepository>();
    }
}
