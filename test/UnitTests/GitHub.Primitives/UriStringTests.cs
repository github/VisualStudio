using System;
using System.Collections.Generic;
using GitHub.Primitives;
using Xunit;

public class UriStringTests
{
    public class TheConstructor : TestBaseClass
    {
        [Theory]
        [InlineData("http://192.168.1.3/foo/bar.git", "192.168.1.3", "foo", "bar")]
        [InlineData("http://haacked@example.com/foo/bar", "example.com", "foo", "bar")]
        [InlineData("http://haacked:password@example.com/foo/bar", "example.com", "foo", "bar")]
        [InlineData("http://example.com/foo/bar.git", "example.com", "foo", "bar")]
        [InlineData("http://example.com/foo/bar", "example.com", "foo", "bar")]
        [InlineData("http://example.com:1234/foo/bar.git", "example.com", "foo", "bar")]
        [InlineData("https://github.com/github/Windows.git", "github.com", "github", "Windows")]
        [InlineData("https://github.com/libgit2/libgit2.github.com", "github.com", "libgit2", "libgit2.github.com")]
        [InlineData("https://github.com/github/Windows", "github.com", "github", "Windows")]
        [InlineData("git@192.168.1.2:github/Windows.git", "192.168.1.2", "github", "Windows")]
        [InlineData("git@github.com:github/Windows.git", "github.com", "github", "Windows")]
        [InlineData("git@github.com:github/Windows", "github.com", "github", "Windows")]
        [InlineData("git@example.com:org/repo.git", "example.com", "org", "repo")]
        [InlineData("ssh://git@github.com:443/benstraub/libgit2", "github.com", "benstraub", "libgit2")]
        [InlineData("git://git@github.com:443/benstraub/libgit2", "github.com", "benstraub", "libgit2")]
        [InlineData("jane;fingerprint=9e:1a:5e:27:16:4d:2a:13:90:2c:64:41:bd:25:fd:35@foo.com:github/Windows.git",
            "foo.com", "github", "Windows")]
        [InlineData("https://haacked@bitbucket.org/haacked/test-mytest.git", "bitbucket.org", "haacked", "test-mytest")]
        [InlineData("https://git01.codeplex.com/nuget", "git01.codeplex.com", null, "nuget")]
        [InlineData("https://example.com/vpath/foo/bar", "example.com", "foo", "bar")]
        [InlineData("https://example.com/vpath/foo/bar.git", "example.com", "foo", "bar")]
        [InlineData("https://github.com/github/Windows.git?pr=24&branch=pr/23&filepath=relative/to/the/path.md",
            "github.com", "github", "Windows")]
        public void ParsesWellFormedUrlComponents(string url, string expectedHost, string owner, string repositoryName)
        {
            var cloneUrl = new UriString(url);

            Assert.Equal(cloneUrl.Host, expectedHost);
            Assert.Equal(cloneUrl.Owner, owner);
            Assert.Equal(cloneUrl.RepositoryName, repositoryName);
            Assert.False(cloneUrl.IsFileUri);
        }

        [Theory]
        [InlineData(@"..\bar\foo")]
        [InlineData(@"..\..\foo")]
        [InlineData(@"../..\foo")]
        [InlineData(@"../../foo")]
        [InlineData(@"..\..\foo.git")]
        [InlineData(@"c:\dev\exp\foo")]
        [InlineData(@"c:\dev\bar\..\exp\foo")]
        [InlineData("c:/dev/exp/foo")]
        [InlineData(@"c:\dev\exp\foo.git")]
        [InlineData("c:/dev/exp/foo.git")]
        [InlineData("c:/dev/exp/bar/../foo.git")]
        [InlineData("file:///C:/dev/exp/foo")]
        [InlineData("file://C:/dev/exp/foo")]
        [InlineData("file://C:/dev/exp/foo.git")]
        public void ParsesLocalFileUris(string path)
        {
            var cloneUrl = new UriString(path);

            Assert.Equal("", cloneUrl.Host);
            Assert.Equal("", cloneUrl.Owner);
            Assert.Equal("foo", cloneUrl.RepositoryName);
            Assert.Equal(cloneUrl.ToString(), path.Replace('\\', '/'));
            Assert.True(cloneUrl.IsFileUri);
        }

        [Theory]
        [InlineData("complete garbage", "", "", null)]
        [InlineData(@"..\other_folder", "", "", "other_folder")]
        [InlineData("http://example.com", "example.com", null, null)]
        [InlineData("http://example.com?bar", "example.com", null, null)]
        [InlineData("https://example.com?bar", "example.com", null, null)]
        [InlineData("ssh://git@example.com/Windows.git", "example.com", null, "Windows")]
        [InlineData("blah@bar.com:/", "bar.com", null, null)]
        [InlineData("blah@bar.com/", "bar.com", null, null)]
        [InlineData("blah@bar.com", "bar.com", null, null)]
        [InlineData("blah@bar.com:/Windows.git", "bar.com", null, "Windows")]
        [InlineData("blah@baz.com/Windows.git", "baz.com", null, "Windows")]
        [InlineData("ssh://git@github.com:github/Windows.git", "github.com", "github", "Windows")]
        public void ParsesWeirdUrlsAsWellAsPossible(string url, string expectedHost, string owner, string repositoryName)
        {
            var cloneUrl = new UriString(url);

            Assert.Equal(cloneUrl.Host, expectedHost);
            Assert.Equal(cloneUrl.Owner, owner);
            Assert.Equal(cloneUrl.RepositoryName, repositoryName);
        }

        [Theory]
        [InlineData(@"http:\\example.com/bar\baz")]
        [InlineData(@"http://example.com/bar/baz")]
        public void NormalizesSeparator(string uriString)
        {
            var path = new UriString(uriString);
            new UriString("http://example.com/bar/baz").Equals(path);
        }

        [Fact]
        public void AcceptsNullConversion()
        {
            Assert.NotNull(new UriString(null));
        }
    }

    public class TheNameWithOwnerProperty : TestBaseClass
    {
        [Theory]
        [InlineData("http://192.168.1.3/foo/bar.git", "foo/bar")]
        [InlineData("http://192.168.1.3/foo/bar", "foo/bar")]
        [InlineData("http://192.168.1.3/foo/bar/baz/qux", "baz/qux")]
        [InlineData("https://github.com/github/Windows.git", "github/Windows")]
        [InlineData("https://github.com/github/", "github")]
        [InlineData("blah@bar.com:/Windows.git", "Windows")]
        [InlineData("git@github.com:github/Windows.git", "github/Windows")]
        public void DependsOnOwnerAndRepoNameNotBeingNull(string url, string expectedNameWithOwner)
        {
            var cloneUrl = new UriString(url);

            Assert.Equal(cloneUrl.NameWithOwner, expectedNameWithOwner);
        }
    }

    public class TheCombineMethod : TestBaseClass
    {
        [Theory]
        [InlineData("http://example.com", "foo/bar", @"http://example.com/foo/bar")]
        [InlineData("http://example.com/", "foo/bar", @"http://example.com/foo/bar")]
        [InlineData("http://example.com/", "/foo/bar/", @"http://example.com/foo/bar/")]
        [InlineData("http://example.com/foo", "bar/", @"http://example.com/foo/bar/")]
        [InlineData("http://example.com", @"foo\bar", @"http://example.com/foo/bar")]
        [InlineData("http://example.com/", @"foo\bar", @"http://example.com/foo/bar")]
        [InlineData("http://example.com/", @"/foo\bar/", @"http://example.com/foo/bar/")]
        [InlineData("http://example.com/foo", @"bar\", @"http://example.com/foo/bar/")]
        [InlineData("http://example.com/foo?bar", @"baz", @"http://example.com/foo?bar&baz")]
        [InlineData("http://example.com/foo?bar", @"&baz", @"http://example.com/foo?bar&baz")]
        [InlineData("http://example.com/foo?", @"bar", @"http://example.com/foo?bar")]
        [InlineData("http://example.com/foo?", @"&bar", @"http://example.com/foo?&bar")]
        public void ComparesHostInsensitively(string uriString, string path, string expected)
        {
            Assert.Equal(new UriString(uriString).Combine(path), (UriString)expected);
        }
    }

    public class TheIsValidUriProperty : TestBaseClass
    {
        [Theory]
        [InlineData("http://example.com/", true)]
        [InlineData("file:///C:/dev/exp/foo", true)]
        [InlineData("garbage", false)]
        [InlineData("git@192.168.1.2:github/Windows.git", false)]
        public void ReturnWhetherTheUriIsParseableByUri(string uriString, bool expected)
        {
            Assert.Equal(new UriString(uriString).IsValidUri, expected);
        }
    }

    public class TheToRepositoryUrlMethod : TestBaseClass
    {
        [Theory]
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
        public void ConvertsToWebUrl(string uriString, string expected)
        {
            Assert.Equal(new Uri(expected), new UriString(uriString).ToRepositoryUrl());
        }

        [Theory]
        [InlineData("file:///C:/dev/exp/foo", "file:///C:/dev/exp/foo")]
        [InlineData("http://example.com/", "http://example.com/")]
        [InlineData("http://haacked@example.com/foo/bar", "http://example.com/baz/bar")]
        [InlineData("https://github.com/github/Windows", "https://github.com/baz/Windows")]
        [InlineData("https://github.com/github/Windows.git", "https://github.com/baz/Windows")]
        [InlineData("https://haacked@github.com/github/Windows.git", "https://github.com/baz/Windows")]
        [InlineData("http://example.com:4000/github/Windows", "http://example.com:4000/baz/Windows")]
        [InlineData("git@192.168.1.2:github/Windows.git", "https://192.168.1.2/baz/Windows")]
        [InlineData("git@example.com:org/repo.git", "https://example.com/baz/repo")]
        [InlineData("ssh://git@github.com:443/shana/cef", "https://github.com/baz/cef")]
        [InlineData("ssh://git@example.com:23/haacked/encourage", "https://example.com:23/baz/encourage")]
        public void ConvertsWithNewOwner(string uriString, string expected)
        {
            Assert.Equal(new Uri(expected), new UriString(uriString).ToRepositoryUrl("baz"));
        }

        [Theory]
        [InlineData("asdf", null)]
        [InlineData("", null)]
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
        public void ShouldNeverThrow(string url, string expected)
        {
            Uri uri;
            Uri.TryCreate(expected, UriKind.Absolute, out uri);
            Assert.Equal(uri, new UriString(url).ToRepositoryUrl());
        }
    }

    public class TheAdditionOperator : TestBaseClass
    {
        [Theory]
        [InlineData("http://example.com", "foo/bar", @"http://example.com/foo/bar")]
        [InlineData("http://example.com/", "foo/bar", @"http://example.com/foo/bar")]
        [InlineData("http://example.com/", "/foo/bar/", @"http://example.com/foo/bar/")]
        [InlineData("http://example.com/foo", "bar/", @"http://example.com/foo/bar/")]
        [InlineData("http://example.com", @"foo\bar", @"http://example.com/foo/bar")]
        [InlineData("http://example.com/", @"foo\bar", @"http://example.com/foo/bar")]
        [InlineData("http://example.com/", @"/foo\bar/", @"http://example.com/foo/bar/")]
        [InlineData("http://example.com/foo", @"bar\", @"http://example.com/foo/bar/")]
        [InlineData("http://example.com/foo?bar", @"baz", @"http://example.com/foo?bar&baz")]
        [InlineData("http://example.com/foo?bar", @"&baz", @"http://example.com/foo?bar&baz")]
        [InlineData("http://example.com/foo?", @"bar", @"http://example.com/foo?bar")]
        [InlineData("http://example.com/foo?", @"&bar", @"http://example.com/foo?&bar")]
        public void CombinesPaths(string uriString, string addition, string expected)
        {
            UriString path = uriString;
            var newPath = path + addition;
            Assert.Equal(newPath, (UriString)expected);
        }
    }

    public class ImplicitConversionToString : TestBaseClass
    {
        [Fact]
        public void ConvertsBackToString()
        {
            var uri = new UriString("http://github.com/foo/bar/");
            string cloneUri = uri;
            Assert.Equal("http://github.com/foo/bar/", cloneUri);
        }

        [Fact]
        public void ConvertsNullToNull()
        {
            UriString uri = null;
            Assert.Null(uri);
            string cloneUri = uri;
            Assert.Null(cloneUri);
        }
    }

    public class ImplicitConversionFromString : TestBaseClass
    {
        [Fact]
        public void ConvertsToCloneUri()
        {
            UriString cloneUri = "http://github.com/foo/bar/";
            Assert.Equal("github.com", cloneUri.Host);
        }

        [Fact]
        public void ConvertsNullToNull()
        {
            UriString cloneUri = (string)null;
            Assert.Null(cloneUri);
        }
    }

    public class TheIsHypertextTransferProtocolProperty : TestBaseClass
    {
        [Theory]
        [InlineData("http://example.com", true)]
        [InlineData("HTTP://example.com", true)]
        [InlineData("https://example.com", true)]
        [InlineData("HTTPs://example.com", true)]
        [InlineData("ftp://example.com", false)]
        [InlineData("c:/example.com", false)]
        [InlineData("git@github.com:github/Windows", false)]
        public void IsTrueOnlyForHttpAndHttps(string url, bool expected)
        {
            var uri = new UriString(url);
            Assert.Equal(uri.IsHypertextTransferProtocol, expected);
        }
    }

    public class TheEqualsMethod : TestBaseClass
    {
        [Theory]
        [InlineData("https://github.com/foo/bar", "https://github.com/foo/bar", true)]
        [InlineData("https://github.com/foo/bar", "https://github.com/foo/BAR", false)]
        [InlineData("https://github.com/foo/bar", "https://github.com/foo/bar/", false)]
        [InlineData("https://github.com/foo/bar", null, false)]
        public void ReturnsTrueForCaseSensitiveEquality(string source, string compare, bool expected)
        {
            Assert.Equal(expected, source.Equals(compare));
            Assert.Equal(expected, EqualityComparer<UriString>.Default.Equals(source, compare));
        }

        [Fact]
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
