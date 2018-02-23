using System;
using GitHub.Services;
using LibGit2Sharp;
using NSubstitute;
using NUnit.Framework;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class GitServiceTests : TestBaseClass
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

        var gitservice = new GitService();
        Assert.That(expected, Is.EqualTo(gitservice.GetUri(repository)?.ToString()));
    }
}
