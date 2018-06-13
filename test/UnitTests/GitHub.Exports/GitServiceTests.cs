using GitHub.Services;
using LibGit2Sharp;
using NSubstitute;
using NUnit.Framework;

public class GitServiceTests : TestBaseClass
{
    public class TheGetUriMethod
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
        public void ShouldNotThrow(string url, string expected)
        {
            var repository = CreateRepository(url);
            var target = new GitService();

            var uri = target.GetUri(repository);

            Assert.That(uri?.ToString(), Is.EqualTo(expected));
        }
    }

    static IRepository CreateRepository(string url, string defaultBranchName = "origin")
    {
        var defaultBranch = Substitute.For<Remote>();
        defaultBranch.Url.Returns(url);
        defaultBranch.Name.Returns(defaultBranchName);
        var repository = Substitute.For<IRepository>();
        repository.Network.Remotes[defaultBranchName].Returns(defaultBranch);
        return repository;
    }
}
