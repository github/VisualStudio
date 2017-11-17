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
        [InlineData("a name", "https://github.com/github/VisualStudio", @"C:\some\path", "a name", "https://github.com/github/VisualStudio", @"C:\some\path")]
        [InlineData("a name", "https://github.com/github/VisualStudio", @"c:\some\path", "a name", "https://github.com/github/VisualStudio", @"C:\some\path")]
        [InlineData("a name", "https://github.com/github/VisualStudio", @"C:\some\path", "a name", "https://github.com/github/VisualStudio", @"c:\some\path")]
        [InlineData("a name", "https://github.com/github/VisualStudio", @"C:\some\path\", "a name", "https://github.com/github/VisualStudio", @"c:\some\path")]
        [InlineData("a name", "https://github.com/github/VisualStudio", @"C:\some\path", "a name", "https://github.com/github/VisualStudio", @"c:\some\path\")]
        [InlineData("a name", "https://github.com/github/VisualStudio", @"C:\some\path\", "a name", "https://github.com/github/VisualStudio", @"c:\some\path\")]
        public void SameContentEqualsTrue(string name1, string url1, string path1, string name2, string url2, string path2)
        {
            var a = new LocalRepositoryModel(name1, new UriString(url1), path1);
            var b = new LocalRepositoryModel(name2, new UriString(url2), path2);
            Assert.Equal(a, b);
            Assert.False(a == b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Theory]
        [InlineData(1, "a name", "https://github.com/github/VisualStudio", 1, "a name", "https://github.com/github/VisualStudio")]
        public void SameContentEqualsTrue2(long id1, string name1, string url1, long id2, string name2, string url2)
        {
            var account = Substitute.For<IAccount>();
            var a = new RemoteRepositoryModel(id1, name1, new UriString(url1), false, false, account, null);
            var b = new RemoteRepositoryModel(id2, name2, new UriString(url2), false, false, account, null);
            Assert.Equal(a, b);
            Assert.False(a == b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Theory]
        [InlineData(1, "a name1", "https://github.com/github/VisualStudio", 2, "a name", "https://github.com/github/VisualStudio")]
        public void DifferentContentEqualsFalse(long id1, string name1, string url1, long id2, string name2, string url2)
        {
            var account = Substitute.For<IAccount>();
            var a = new RemoteRepositoryModel(id1, name1, new UriString(url1), false, false, account, null);
            var b = new RemoteRepositoryModel(id2, name2, new UriString(url2), false, false, account, null);
            Assert.NotEqual(a, b);
            Assert.False(a == b);
            Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
        }
    }

    [Collection("PackageServiceProvider global data tests")]
    public class PathConstructorTests : TestBaseClass
    {
        [Fact]
        public void NoRemoteUrl()
        {
            using (var temp = new TempDirectory())
            {
                var provider = Substitutes.ServiceProvider;
                var gitservice = provider.GetGitService();
                var repo = Substitute.For<IRepository>();
                var path = temp.Directory.CreateSubdirectory("repo-name");
                gitservice.GetUri(path.FullName).Returns((UriString)null);
                var model = new LocalRepositoryModel(path.FullName);
                Assert.Equal("repo-name", model.Name);
            }
        }

        [Fact]
        public void WithRemoteUrl()
        {
            using (var temp = new TempDirectory())
            {
                var provider = Substitutes.ServiceProvider;
                var gitservice = provider.GetGitService();
                var repo = Substitute.For<IRepository>();
                var path = temp.Directory.CreateSubdirectory("repo-name");
                gitservice.GetUri(path.FullName).Returns(new UriString("https://github.com/user/repo-name"));
                var model = new LocalRepositoryModel(path.FullName);
                Assert.Equal("repo-name", model.Name);
                Assert.Equal("user", model.Owner);
            }
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
