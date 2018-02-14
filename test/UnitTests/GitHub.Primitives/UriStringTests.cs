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
        [TestCase("https://git01.codeplex.com/nuget", "git01.codeplex.com", null, "nuget")]
        [TestCase("https://example.com/vpath/foo/bar", "example.com", "foo", "bar")]
        [TestCase("https://example.com/vpath/foo/bar.git", "example.com", "foo", "bar")]
        [TestCase("https://github.com/github/Windows.git?pr=24&branch=pr/23&filepath=relative/to/the/path.md",
            "github.com", "github", "Windows")]
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
        [TestCase("ssh://git@example.com/Windows.git", "example.com", null, "Windows")]
        [TestCase("blah@bar.com:/", "bar.com", null, null)]
        [TestCase("blah@bar.com/", "bar.com", null, null)]
        [TestCase("blah@bar.com", "bar.com", null, null)]
        [TestCase("blah@bar.com:/Windows.git", "bar.com", null, "Windows")]
        [TestCase("blah@baz.com/Windows.git", "baz.com", null, "Windows")]
        [TestCase("ssh://git@github.com:github/Windows.git", "github.com", "github", "Windows")]
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
        [TestCase("http://192.168.1.3/foo/bar/baz/qux", "baz/qux")]
        [TestCase("https://github.com/github/Windows.git", "github/Windows")]
        [TestCase("https://github.com/github/", "github")]
        [TestCase("blah@bar.com:/Windows.git", "Windows")]
        [TestCase("git@github.com:github/Windows.git", "github/Windows")]
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
        public void ConvertsToWebUrl(string uriString, string expected)
        {
            Assert.That(new Uri(expected), Is.EqualTo(new UriString(uriString).ToRepositoryUrl()));
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
            Assert.That(expected, Is.EqualTo(source.Equals(compare)));
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
}
