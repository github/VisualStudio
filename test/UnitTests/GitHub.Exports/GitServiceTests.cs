using System.Collections.Generic;
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
            var repository = CreateRepository(new[] { url }, new[] { "origin" });
            var target = new GitService();

            var uri = target.GetUri(repository);

            Assert.That(uri?.ToString(), Is.EqualTo(expected));
        }

        [TestCase("https://github.com/github/VisualStudio", "origin", "https://github.com/github/VisualStudio")]
        [TestCase("https://github.com/github/VisualStudio", "renamed", "https://github.com/github/VisualStudio")]
        [TestCase("", "", null, Description = "No remotes returns null")]
        [TestCase("https://github.com/github/VisualStudio;https://github.com/jcansdale/VisualStudio", "github;jcansdale",
            "https://github.com/github/VisualStudio", Description = "Return first remote when no origin")]
        public void DifferentRemoteNames(string urls, string remoteNames, string expected)
        {
            var repository = CreateRepository(urls.Split(';'), remoteNames.Split(';'));
            var target = new GitService();

            var uri = target.GetUri(repository);

            Assert.That(uri?.ToString(), Is.EqualTo(expected));
        }
    }

    static IRepository CreateRepository(string[] urls, string[] remoteNames)
    {
        var repository = Substitute.For<IRepository>();
        var remoteCollection = Substitute.For<RemoteCollection>();

        var remoteList = new List<Remote>();
        for (var count = 0; count < urls.Length; count++)
        {
            var url = urls[count];
            var remoteName = remoteNames[count];
            var remote = Substitute.For<Remote>();
            remote.Url.Returns(url);
            remote.Name.Returns(remoteName);
            remoteCollection[remoteName].Returns(remote);
            remoteList.Add(remote);
        }

        remoteCollection.GetEnumerator().Returns(_ => remoteList.GetEnumerator());
        repository.Network.Remotes.Returns(remoteCollection);
        return repository;
    }
}
