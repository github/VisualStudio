using System;
using System.Collections.Generic;
using GitHub.Services;
using LibGit2Sharp;
using NSubstitute;
using NUnit.Framework;

public class GitServiceTests
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
            var repository = CreateRepository(new[] { url }, new[] { "origin" }, new string[0], new string[0]);
            var target = new GitService();

            var uri = target.GetUri(repository);

            Assert.That(uri?.ToString(), Is.EqualTo(expected));
        }

        [TestCase("https://github.com/github/VisualStudio", "no_origin", "no_master", "github", Description = "No `origin` remote or `master` branch defined")]
        public void UriShouldBeNull(string urls, string remoteNames, string branchNames, string branchRemoteNames)
        {
            var repository = CreateRepository(Split(urls), Split(remoteNames), Split(branchNames), Split(branchRemoteNames));
            var target = new GitService();

            var uri = target.GetUri(repository);

            Assert.That(uri, Is.Null);
        }

        [Test]
        public void RepositoryNull_UriShouldBeNull()
        {
            var repository = null as IRepository;
            var target = new GitService();

            var uri = target.GetUri(repository);

            Assert.That(uri, Is.Null);
        }
    }

    public class TheGetOriginRemoteNameMethod
    {
        [TestCase("https://github.com/github/VisualStudio", "no_origin", "no_master", "github", Description = "No `origin` remote or `master` branch defined")]
        [TestCase("https://github.com/jcansdale/VisualStudio", "jcansdale", "master", "jcansdale", Description = "Don't use remote from branch named `master`")]
        public void ThrowsInvalidOperationException(string urls, string remoteNames, string branchNames, string branchRemoteNames)
        {
            var repository = CreateRepository(Split(urls), Split(remoteNames), Split(branchNames), Split(branchRemoteNames));
            var target = new GitService();

            Assert.Throws<InvalidOperationException>(() => target.GetDefaultRemoteName(repository));
        }

        [TestCase("https://github.com/github/VisualStudio", "origin", "master;HEAD", "jcansdale;grokys", "origin",
            Description = "Use remote named `origin` if it exists")]
        public void GetOriginRemoteName(string urls, string remoteNames, string branchNames, string branchRemoteNames, string expectedRemoteName)
        {
            var repository = CreateRepository(Split(urls), Split(remoteNames), Split(branchNames), Split(branchRemoteNames));
            var target = new GitService();

            var remoteName = target.GetDefaultRemoteName(repository);

            Assert.That(remoteName, Is.EqualTo(expectedRemoteName));
        }

        [TestCase("https://github.com/github/VisualStudio", "configured_origin", "configured_origin", "configured_origin",
            Description = "Use remote from `" + GitService.OriginConfigKey + "` config setting if it exists")]
        [TestCase("https://github.com/github/VisualStudio;https://github.com/jcansdale/VisualStudio", "origin;configured_origin",
            "configured_origin", "configured_origin", Description = "Allow user to override the default origin remote")]
        public void GetOriginFromConfig(string urls, string remoteNames, string configOrigin, string expectedRemoteName)
        {
            var config = CreateConfiguration(configOrigin);
            var repository = CreateRepository(Split(urls), Split(remoteNames), Split(""), Split(""), config);
            var target = new GitService();

            var remoteName = target.GetDefaultRemoteName(repository);

            Assert.That(remoteName, Is.EqualTo(expectedRemoteName));
        }
    }

    static string[] Split(string text)
    {
        return text.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
    }

    static IRepository CreateRepository(string[] urls, string[] remoteNames, string[] branchNames, string[] branchRemoteNames,
        Configuration config = null)
    {
        config = config ?? Substitute.For<Configuration>();

        var repository = Substitute.For<IRepository>();
        repository.Config.Returns(config);

        var remoteCollection = Substitute.For<RemoteCollection>();

        for (var branchCount = 0; branchCount < branchNames.Length; branchCount++)
        {
            var branchName = branchNames[branchCount];
            var branchRemoteName = branchRemoteNames[branchCount];
            var branch = Substitute.For<Branch>();
            branch.RemoteName.Returns(branchRemoteName);
            repository.Branches[branchName].Returns(branch);
            if (branchName == "HEAD")
            {
                repository.Head.Returns(branch);
            }
        }

        var remoteList = new List<Remote>();
        for (var remoteCount = 0; remoteCount < urls.Length; remoteCount++)
        {
            var url = urls[remoteCount];
            var remoteName = remoteNames[remoteCount];
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

    static Configuration CreateConfiguration(string configOrigin)
    {
        var config = Substitute.For<Configuration>();
        config.GetValueOrDefault<string>(GitService.OriginConfigKey).Returns(configOrigin);
        return config;
    }
}
