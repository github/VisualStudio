using System;
using GitHub.Services;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using NSubstitute;
using Xunit;
using GitHub.Extensions;

public class URITests
{
    public class TheGetUserMethod : TestBaseClass
    {
        [Theory]
        [InlineData("git://test/athing", null)]
        [InlineData("git://test/user/athing", "user")]
        public void OnlyParsesUrlsWithThreeParts(string uri, string expected)
        {
            Assert.Equal(new Uri(uri).GetUser(), expected);
        }
    }

    public class TheGetRepoMethod : TestBaseClass
    {
        [Theory]
        [InlineData("git://test/athing", null)]
        [InlineData("git://test/user/athing", "athing")]
        public void OnlyParsesUrlsWithThreeParts(string uri, string expected)
        {
            Assert.Equal(new Uri(uri).GetRepo(), expected);
        }
    }

    public class TheHttpsMethod : TestBaseClass
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData("git@github.com:bla/test.git", "https://github.com/bla/test")]
        [InlineData("git://github.com/bla/test.git", "https://github.com/bla/test")]
        public void CreatesHttpsUrl(string uri, string expected)
        {
            Uri value = null;
            Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out value);

            var ret = value.ToHttps();
            var ret2 = ret != null ? ret.ToString() : null;
            Assert.Equal(expected, ret2);
        }
    }
}
