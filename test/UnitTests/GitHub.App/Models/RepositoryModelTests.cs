using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.VisualStudio;
using LibGit2Sharp;
using NSubstitute;
using UnitTests;
using NUnit.Framework;

public class RepositoryModelTests
{
    public class ComparisonTests : TestBaseClass
    {
        [TestCase("a name", "https://github.com/github/VisualStudio", @"C:\some\path", "a name", "https://github.com/github/VisualStudio", @"C:\some\path")]
        [TestCase("a name", "https://github.com/github/VisualStudio", @"c:\some\path", "a name", "https://github.com/github/VisualStudio", @"C:\some\path")]
        [TestCase("a name", "https://github.com/github/VisualStudio", @"C:\some\path", "a name", "https://github.com/github/VisualStudio", @"c:\some\path")]
        [TestCase("a name", "https://github.com/github/VisualStudio", @"C:\some\path\", "a name", "https://github.com/github/VisualStudio", @"c:\some\path")]
        [TestCase("a name", "https://github.com/github/VisualStudio", @"C:\some\path", "a name", "https://github.com/github/VisualStudio", @"c:\some\path\")]
        [TestCase("a name", "https://github.com/github/VisualStudio", @"C:\some\path\", "a name", "https://github.com/github/VisualStudio", @"c:\some\path\")]
        public void SameContentEqualsTrue(string name1, string url1, string path1, string name2, string url2, string path2)
        {
            var a = new LocalRepositoryModel(name1, new UriString(url1), path1);
            var b = new LocalRepositoryModel(name2, new UriString(url2), path2);
            Assert.That(a, Is.EqualTo(b));
            Assert.False(a == b);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [TestCase(1, "a name", "https://github.com/github/VisualStudio", 1, "a name", "https://github.com/github/VisualStudio")]
        public void SameContentEqualsTrue2(long id1, string name1, string url1, long id2, string name2, string url2)
        {
            var account = Substitute.For<IAccount>();
            var a = new RemoteRepositoryModel(id1, name1, new UriString(url1), false, false, account, null);
            var b = new RemoteRepositoryModel(id2, name2, new UriString(url2), false, false, account, null);
            Assert.That(a, Is.EqualTo(b));
            Assert.False(a == b);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [TestCase(1, "a name1", "https://github.com/github/VisualStudio", 2, "a name", "https://github.com/github/VisualStudio")]
        public void DifferentContentEqualsFalse(long id1, string name1, string url1, long id2, string name2, string url2)
        {
            var account = Substitute.For<IAccount>();
            var a = new RemoteRepositoryModel(id1, name1, new UriString(url1), false, false, account, null);
            var b = new RemoteRepositoryModel(id2, name2, new UriString(url2), false, false, account, null);
            Assert.That(a, Is.Not.EqualTo(b));
            Assert.False(a == b);
            Assert.That(a.GetHashCode(), Is.Not.EqualTo(b.GetHashCode()));
        }
    }

    //[Collection("PackageServiceProvider global data tests")]
    public class PathConstructorTests : TestBaseClass
    {
        [Test]
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
                Assert.That("repo-name", Is.EqualTo(model.Name));
            }
        }

        [Test]
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
                Assert.That("repo-name", Is.EqualTo(model.Name));
                Assert.That("user", Is.EqualTo(model.Owner));
            }
        }
    }

    public class HostAddressTests : TestBaseClass
    {
        [TestCase("https://github.com/owner/repo")]
        [TestCase("https://anotherurl.com/foo/bar")]
        public void SameContentEqualsTrue(string url)
        {
            var a = HostAddress.Create(url);
            var b = HostAddress.Create(url);
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }
    }
}
