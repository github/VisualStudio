using System;
using GitHub.Services;
using LibGit2Sharp;
using NSubstitute;
using Xunit;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class GitServiceTests : TestBaseClass
{
    [Theory]
    [InlineData("asdf", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    [InlineData("file:///C:/dev/exp/foo", "file:///C:/dev/exp/foo")]
    [InlineData("http://example.com/", "http://example.com/")]
    [InlineData("http://haacked@example.com/foo/bar", "http://example.com/foo/bar")]
    [InlineData("https://github.com/github/Windows", "https://github.com/github/Windows")]
    [InlineData("https://github.com/github/Windows.git", "https://github.com/github/Windows")]
    [InlineData("https://haacked@github.com/github/Windows.git", "https://github.com/github/Windows")]
    [InlineData("http://example.com:4000/github/Windows", "http://example.com:4000/github/Windows")]
    [InlineData("git@192.168.1.2:github/Windows.git", "https://192.168.1.2/github/Windows")]
    [InlineData("git@example.com:org/repo.git", "https://example.com/org/repo")]
    [InlineData("ssh://git@github.com:443/shana/cef", "https://github.com/shana/cef")]
    [InlineData("ssh://git@example.com:23/haacked/encourage", "https://example.com:23/haacked/encourage")]
    public void GetUriShouldNotThrow(string url, string expected)
    {
        var origin = Substitute.For<Remote>();
        origin.Url.Returns(url);
        var repository = Substitute.For<IRepository>();
        repository.Network.Remotes["origin"].Returns(origin);

        var gitservice = new GitService();
        Assert.Equal(expected, gitservice.GetUri(repository)?.ToString());
    }
}
