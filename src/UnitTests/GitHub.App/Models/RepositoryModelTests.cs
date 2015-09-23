using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.VisualStudio;
using LibGit2Sharp;
using NSubstitute;
using UnitTests;
using Xunit;

public class RepositoryModelTests
{
    public class ComparisonTests : TestBaseClass
    {
        [Theory]
        [InlineData("a name", "https://github.com/github/VisualStudio", null, "a name", "https://github.com/github/VisualStudio", null)]
        [InlineData("a name", "https://github.com/github/VisualStudio", @"C:\some\path", "a name", "https://github.com/github/VisualStudio", @"C:\some\path")]
        [InlineData("a name", "https://github.com/github/VisualStudio", @"c:\some\path", "a name", "https://github.com/github/VisualStudio", @"C:\some\path")]
        [InlineData("a name", "https://github.com/github/VisualStudio", @"C:\some\path", "a name", "https://github.com/github/VisualStudio", @"c:\some\path")]
        [InlineData("a name", "https://github.com/github/VisualStudio", @"C:\some\path\", "a name", "https://github.com/github/VisualStudio", @"c:\some\path")]
        [InlineData("a name", "https://github.com/github/VisualStudio", @"C:\some\path", "a name", "https://github.com/github/VisualStudio", @"c:\some\path\")]
        [InlineData("a name", "https://github.com/github/VisualStudio", @"C:\some\path\", "a name", "https://github.com/github/VisualStudio", @"c:\some\path\")]
        public void SameContentEqualsTrue(string name1, string url1, string path1, string name2, string url2, string path2)
        {
            var a = new SimpleRepositoryModel(name1, new UriString(url1), path1);
            var b = new SimpleRepositoryModel(name2, new UriString(url2), path2);
            Assert.Equal(a, b);
            Assert.False(a == b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Theory]
        [InlineData("a name", "https://github.com/github/VisualStudio", "a name", "https://github.com/github/VisualStudio")]
        public void SameContentEqualsTrue2(string name1, string url1, string name2, string url2)
        {
            var account = Substitute.For<IAccount>();
            var a = new RepositoryModel(name1, new UriString(url1), false, false, account);
            var b = new RepositoryModel(name2, new UriString(url2), false, false, account);
            Assert.Equal(a, b);
            Assert.False(a == b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Theory]
        [InlineData("a name1", "https://github.com/github/VisualStudio", "a name", "https://github.com/github/VisualStudio")]
        public void DifferentContentEqualsFalse(string name1, string url1, string name2, string url2)
        {
            var account = Substitute.For<IAccount>();
            var a = new RepositoryModel(name1, new UriString(url1), false, false, account);
            var b = new RepositoryModel(name2, new UriString(url2), false, false, account);
            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
        }
    }

    public class PathConstructorTests : TempFileBaseClass
    {
        [Fact]
        public void NoRemoteUrl()
        {
            var provider = Substitutes.ServiceProvider;
            Services.PackageServiceProvider = provider;
            var gitservice = provider.GetGitService();
            var repo = Substitute.For<IRepository>();
            var path = Directory.CreateSubdirectory("repo-name");
            gitservice.GetUri(path.FullName).Returns((UriString)null);
            var model = new SimpleRepositoryModel(path.FullName);
            Assert.Equal("repo-name", model.Name);
        }

        [Fact]
        public void WithRemoteUrl()
        {
            var provider = Substitutes.ServiceProvider;
            Services.PackageServiceProvider = provider;
            var gitservice = provider.GetGitService();
            var repo = Substitute.For<IRepository>();
            var path = Directory.CreateSubdirectory("repo-name");
            gitservice.GetUri(path.FullName).Returns(new UriString("https://github.com/user/repo-name"));
            var model = new SimpleRepositoryModel(path.FullName);
            Assert.Equal("user/repo-name", model.Name);
        }
    }

    public class HostAddressTests : TestBaseClass
    {
        [Theory]
        [InlineData("https://github.com/owner/repo")]
        [InlineData("https://anotherurl.com/foo/bar")]
        public void SameContentEqualsTrue(string url)
        {
            var a = HostAddress.Create(url);
            var b = HostAddress.Create(url);
            Assert.Equal(a, b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }
    }
}
