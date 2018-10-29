using System;
using System.Collections.Generic;
using GitHub.Primitives;
using NUnit.Framework;

public class UriStringTests
{
    public class TheConstructor : TestBaseClass
    {
        [TestCase("http://192.168.1.3/foo/bar.git", "192.168.1.3", "foo", "bar")]
        [TestCase("http://haacked@example.com/foo/bar", "example.com", "foo", "bar")]
        [TestCase("http://haacked:password@example.com/foo/bar", "example.com", "foo", "bar")]
        [TestCase("http://example.com/foo/bar.git", "example.com", "foo", "bar")]
        [TestCase("http://example.com/foo/bar", "example.com", "foo", "bar")]
        [TestCase("http://example.com:1234/foo/bar.git", "example.com", "foo", "bar")]
        [TestCase("https://github.com/github/Windows.git", "github.com", "github", "Windows")]
        [TestCase("https://github.com/libgit2/libgit2.github.com", "github.com", "libgit2", "libgit2.github.com")]
        [TestCase("https://github.com/github/Windows", "github.com", "github", "Windows")]
        [TestCase("git@192.168.1.2:github/Windows.git", "192.168.1.2", "github", "Windows")]
        [TestCase("git@github.com:github/Windows.git", "github.com", "github", "Windows")]
        [TestCase("git@github.com:github/Windows", "github.com", "github", "Windows")]
        [TestCase("git@example.com:org/repo.git", "example.com", "org", "repo")]
        [TestCase("ssh://git@github.com:443/benstraub/libgit2", "github.com", "benstraub", "libgit2")]
        [TestCase("git://git@github.com:443/benstraub/libgit2", "github.com", "benstraub", "libgit2")]
        [TestCase("jane;fingerprint=9e:1a:5e:27:16:4d:2a:13:90:2c:64:41:bd:25:fd:35@foo.com:github/Windows.git",
            "foo.com", "github", "Windows")]
        [TestCase("https://haacked@bitbucket.org/haacked/test-mytest.git", "bitbucket.org", "haacked", "test-mytest")]
        [TestCase("https://git01.codeplex.com/nuget", "git01.codeplex.com", "nuget", null,
            Description = "We assume the first component is the owner")]
        [TestCase("https://github.com/github/Windows.git?pr=24&branch=pr/23&filepath=relative/to/the/path.md",
            "github.com", "github", "Windows")]
        [TestCase("https://github.com/github/VisualStudio/blob/master/src/code.cs", "github.com", "github", "VisualStudio")]
        [TestCase("https://github.com/github", "github.com", "github", null)]
        [TestCase("https://github.com", "github.com", null, null)]
        public void ParsesWellFormedUrlComponents(string url, string expectedHost, string owner, string repositoryName)
        {
            var cloneUrl = new UriString(url);

            Assert.That(cloneUrl.Host, Is.EqualTo(expectedHost));
            Assert.That(cloneUrl.Owner, Is.EqualTo(owner));
            Assert.That(cloneUrl.RepositoryName, Is.EqualTo(repositoryName));
            Assert.False(cloneUrl.IsFileUri);
        }

        [TestCase(@"..\bar\foo")]
        [TestCase(@"..\..\foo")]
        [TestCase(@"../..\foo")]
        [TestCase(@"../../foo")]
        [TestCase(@"..\..\foo.git")]
        [TestCase(@"c:\dev\exp\foo")]
        [TestCase(@"c:\dev\bar\..\exp\foo")]
        [TestCase("c:/dev/exp/foo")]
        [TestCase(@"c:\dev\exp\foo.git")]
        [TestCase("c:/dev/exp/foo.git")]
        [TestCase("c:/dev/exp/bar/../foo.git")]
        [TestCase("file:///C:/dev/exp/foo")]
        [TestCase("file://C:/dev/exp/foo")]
        [TestCase("file://C:/dev/exp/foo.git")]
        public void ParsesLocalFileUris(string path)
        {
            var cloneUrl = new UriString(path);

            Assert.That("", Is.EqualTo(cloneUrl.Host));
            Assert.That("", Is.EqualTo(cloneUrl.Owner));
            Assert.That("foo", Is.EqualTo(cloneUrl.RepositoryName));
            Assert.That(cloneUrl.ToString(), Is.EqualTo(path.Replace('\\', '/')));
            Assert.True(cloneUrl.IsFileUri);
        }

        [TestCase("complete garbage", "", "", null)]
        [TestCase(@"..\other_folder", "", "", "other_folder")]
        [TestCase("http://example.com", "example.com", null, null)]
        [TestCase("http://example.com?bar", "example.com", null, null)]
        [TestCase("https://example.com?bar", "example.com", null, null)]
        [TestCase("ssh://git@example.com/Windows.git", "example.com", "Windows.git", null,
            Description = "We assume the first component is the owner even if it ends with .git")]
        [TestCase("blah@bar.com:/", "bar.com", null, null)]
        [TestCase("blah@bar.com/", "bar.com", null, null)]
        [TestCase("blah@bar.com", "bar.com", null, null)]
        [TestCase("blah@bar.com:/Windows.git", "bar.com", null, "Windows")]
        [TestCase("blah@baz.com/Windows.git", "baz.com", null, "Windows")]
        [TestCase("ssh://git@github.com:github/Windows.git", "github.com", "github", "Windows")]

        // NOTE: Used by LocalRepositoryModelTests.GenerateUrl but I don't think it's a legal URL
        [TestCase("git@github.com/foo/bar", "github.com", null, "foo/bar")]
        public void ParsesWeirdUrlsAsWellAsPossible(string url, string expectedHost, string owner, string repositoryName)
        {
            var cloneUrl = new UriString(url);

            Assert.That(cloneUrl.Host, Is.EqualTo(expectedHost));
            Assert.That(cloneUrl.Owner, Is.EqualTo(owner));
            Assert.That(cloneUrl.RepositoryName, Is.EqualTo(repositoryName));
        }

        [TestCase(@"http:\\example.com/bar\baz")]
        [TestCase(@"http://example.com/bar/baz")]
        public void NormalizesSeparator(string uriString)
        {
            var path = new UriString(uriString);
            new UriString("http://example.com/bar/baz").Equals(path);
        }

        [Test]
        public void AcceptsNullConversion()
        {
            Assert.That(new UriString(null), Is.Not.Null);
        }
    }

    public class TheNameWithOwnerProperty : TestBaseClass
    {
        [TestCase("http://192.168.1.3/foo/bar.git", "foo/bar")]
        [TestCase("http://192.168.1.3/foo/bar", "foo/bar")]
        [TestCase("http://192.168.1.3/foo/bar/baz/qux", "foo/bar")]
        [TestCase("https://github.com/github/Windows.git", "github/Windows")]
        [TestCase("https://github.com/github/", "github")]
        [TestCase("blah@bar.com:/Windows.git", "Windows")]
        [TestCase("git@github.com:github/Windows.git", "github/Windows")]
        [TestCase("https://github.com/github/VisualStudio/blob/master/src/code.cs", "github/VisualStudio")]
        public void DependsOnOwnerAndRepoNameNotBeingNull(string url, string expectedNameWithOwner)
        {
            var cloneUrl = new UriString(url);

            Assert.That(cloneUrl.NameWithOwner, Is.EqualTo(expectedNameWithOwner));
        }
    }

    public class TheCombineMethod : TestBaseClass
    {
        [TestCase("http://example.com", "foo/bar", @"http://example.com/foo/bar")]
        [TestCase("http://example.com/", "foo/bar", @"http://example.com/foo/bar")]
        [TestCase("http://example.com/", "/foo/bar/", @"http://example.com/foo/bar/")]
        [TestCase("http://example.com/foo", "bar/", @"http://example.com/foo/bar/")]
        [TestCase("http://example.com", @"foo\bar", @"http://example.com/foo/bar")]
        [TestCase("http://example.com/", @"foo\bar", @"http://example.com/foo/bar")]
        [TestCase("http://example.com/", @"/foo\bar/", @"http://example.com/foo/bar/")]
        [TestCase("http://example.com/foo", @"bar\", @"http://example.com/foo/bar/")]
        [TestCase("http://example.com/foo?bar", @"baz", @"http://example.com/foo?bar&baz")]
        [TestCase("http://example.com/foo?bar", @"&baz", @"http://example.com/foo?bar&baz")]
        [TestCase("http://example.com/foo?", @"bar", @"http://example.com/foo?bar")]
        [TestCase("http://example.com/foo?", @"&bar", @"http://example.com/foo?&bar")]
        public void ComparesHostInsensitively(string uriString, string path, string expected)
        {
            Assert.That(new UriString(uriString).Combine(path), Is.EqualTo((UriString)expected));
        }
    }

    public class TheIsValidUriProperty : TestBaseClass
    {
        [TestCase("http://example.com/", true)]
        [TestCase("file:///C:/dev/exp/foo", true)]
        [TestCase("garbage", false)]
        [TestCase("git@192.168.1.2:github/Windows.git", false)]
        public void ReturnWhetherTheUriIsParseableByUri(string uriString, bool expected)
        {
            Assert.That(new UriString(uriString).IsValidUri, Is.EqualTo(expected));
        }
    }

    public class TheToRepositoryUrlMethod : TestBaseClass
    {
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
        [TestCase("https://github.com/github/VisualStudio/blob/master/src/code.cs", "https://github.com/github/VisualStudio")]
        public void ConvertsToWebUrl(string uriString, string expected)
        {
            Assert.That(new UriString(uriString).ToRepositoryUrl(), Is.EqualTo(new Uri(expected)));
        }

        [TestCase("file:///C:/dev/exp/foo", "file:///C:/dev/exp/foo")]
        [TestCase("http://example.com/", "http://example.com/")]
        [TestCase("http://haacked@example.com/foo/bar", "http://example.com/baz/bar")]
        [TestCase("https://github.com/github/Windows", "https://github.com/baz/Windows")]
        [TestCase("https://github.com/github/Windows.git", "https://github.com/baz/Windows")]
        [TestCase("https://haacked@github.com/github/Windows.git", "https://github.com/baz/Windows")]
        [TestCase("http://example.com:4000/github/Windows", "http://example.com:4000/baz/Windows")]
        [TestCase("git@192.168.1.2:github/Windows.git", "https://192.168.1.2/baz/Windows")]
        [TestCase("git@example.com:org/repo.git", "https://example.com/baz/repo")]
        [TestCase("ssh://git@github.com:443/shana/cef", "https://github.com/baz/cef")]
        [TestCase("ssh://git@example.com:23/haacked/encourage", "https://example.com:23/baz/encourage")]
        [TestCase("https://github.com/github", "https://github.com/github")]
        [TestCase("https://github.com/github/Windows", "https://github.com/github/Windows", null)]
        public void ConvertsWithNewOwner(string uriString, string expected, string owner = "baz")
        {
            Assert.That(new UriString(uriString).ToRepositoryUrl(owner), Is.EqualTo(new Uri(expected)));
        }

        [TestCase("asdf", null)]
        [TestCase("", null)]
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
        public void ShouldNeverThrow(string url, string expected)
        {
            Uri uri;
            Uri.TryCreate(expected, UriKind.Absolute, out uri);
            Assert.That(uri, Is.EqualTo(new UriString(url).ToRepositoryUrl()));
        }
    }

    public class TheAdditionOperator : TestBaseClass
    {
        [TestCase("http://example.com", "foo/bar", @"http://example.com/foo/bar")]
        [TestCase("http://example.com/", "foo/bar", @"http://example.com/foo/bar")]
        [TestCase("http://example.com/", "/foo/bar/", @"http://example.com/foo/bar/")]
        [TestCase("http://example.com/foo", "bar/", @"http://example.com/foo/bar/")]
        [TestCase("http://example.com", @"foo\bar", @"http://example.com/foo/bar")]
        [TestCase("http://example.com/", @"foo\bar", @"http://example.com/foo/bar")]
        [TestCase("http://example.com/", @"/foo\bar/", @"http://example.com/foo/bar/")]
        [TestCase("http://example.com/foo", @"bar\", @"http://example.com/foo/bar/")]
        [TestCase("http://example.com/foo?bar", @"baz", @"http://example.com/foo?bar&baz")]
        [TestCase("http://example.com/foo?bar", @"&baz", @"http://example.com/foo?bar&baz")]
        [TestCase("http://example.com/foo?", @"bar", @"http://example.com/foo?bar")]
        [TestCase("http://example.com/foo?", @"&bar", @"http://example.com/foo?&bar")]
        public void CombinesPaths(string uriString, string addition, string expected)
        {
            UriString path = uriString;
            var newPath = path + addition;
            Assert.That(newPath, Is.EqualTo((UriString)expected));
        }
    }

    public class ImplicitConversionToString : TestBaseClass
    {
        [Test]
        public void ConvertsBackToString()
        {
            var uri = new UriString("http://github.com/foo/bar/");
            string cloneUri = uri;
            Assert.That("http://github.com/foo/bar/", Is.EqualTo(cloneUri));
        }

        [Test]
        public void ConvertsNullToNull()
        {
            UriString uri = null;
            Assert.That(uri, Is.Null);
            string cloneUri = uri;
            Assert.That(cloneUri, Is.Null);
        }
    }

    public class ImplicitConversionFromString : TestBaseClass
    {
        [Test]
        public void ConvertsToCloneUri()
        {
            UriString cloneUri = "http://github.com/foo/bar/";
            Assert.That("github.com", Is.EqualTo(cloneUri.Host));
        }

        [Test]
        public void ConvertsNullToNull()
        {
            UriString cloneUri = (string)null;
            Assert.That(cloneUri, Is.Null);
        }
    }

    public class TheIsHypertextTransferProtocolProperty : TestBaseClass
    {
        [TestCase("http://example.com", true)]
        [TestCase("HTTP://example.com", true)]
        [TestCase("https://example.com", true)]
        [TestCase("HTTPs://example.com", true)]
        [TestCase("ftp://example.com", false)]
        [TestCase("c:/example.com", false)]
        [TestCase("git@github.com:github/Windows", false)]
        public void IsTrueOnlyForHttpAndHttps(string url, bool expected)
        {
            var uri = new UriString(url);
            Assert.That(uri.IsHypertextTransferProtocol, Is.EqualTo(expected));
        }
    }

    public class TheEqualsMethod : TestBaseClass
    {
        [TestCase("https://github.com/foo/bar", "https://github.com/foo/bar", true)]
        [TestCase("https://github.com/foo/bar", "https://github.com/foo/BAR", false)]
        [TestCase("https://github.com/foo/bar", "https://github.com/foo/bar/", false)]
        [TestCase("https://github.com/foo/bar", null, false)]
        public void ReturnsTrueForCaseSensitiveEquality(string source, string compare, bool expected)
        {
            Assert.That(expected, Is.EqualTo(source.Equals(compare, StringComparison.Ordinal)));
            Assert.That(expected, Is.EqualTo(EqualityComparer<UriString>.Default.Equals(source, compare)));
        }

        [Test]
        public void MakesUriStringSuitableForDictionaryKey()
        {
            var dictionary = new Dictionary<UriString, string>
            {
                { new UriString("https://github.com/foo/bar"), "whatever" }
            };

            Assert.False(dictionary.ContainsKey("https://github.com/foo/not-bar"));
            Assert.True(dictionary.ContainsKey("https://github.com/foo/bar"));
            Assert.True(dictionary.ContainsKey(new UriString("https://github.com/foo/bar")));
        }
    }

    public class TheRepositoryUrlsAreEqualMethod
    {
        [TestCase("https://github.com/owner/repo", "https://github.com/owner/repo", true)]
        [TestCase("https://github.com/owner/repo", "HTTPS://GITHUB.COM/OWNER/REPO", true)]
        [TestCase("https://github.com/owner/repo.git", "https://github.com/owner/repo", true)]
        [TestCase("https://github.com/owner/repo", "https://github.com/owner/repo.git", true)]
        [TestCase("https://github.com/owner/repo", "https://github.com/different_owner/repo", false)]
        [TestCase("https://github.com/owner/repo", "https://github.com/owner/different_repo", false)]
        [TestCase("ssh://git@github.com:443/shana/cef", "https://github.com/shana/cef", false)]
        [TestCase("file://github.com/github/visualstudio", "https://github.com/github/visualstudio", false)]
        [TestCase("http://github.com/github/visualstudio", "https://github.com/github/visualstudio", false,
            Description = "http is different to https")]
        [TestCase("ssh://git@github.com:443/shana/cef", "ssh://git@github.com:443/shana/cef", false,
            Description = "The same but not a repository URL")]
        public void RepositoryUrlsAreEqual(string url1, string url2, bool expectEqual)
        {
            var uriString1 = new UriString(url1);
            var uriString2 = new UriString(url2);

            var equal = UriString.RepositoryUrlsAreEqual(uriString1, uriString2);

            Assert.That(equal, Is.EqualTo(expectEqual));
        }
    }
}
