using System;
using GitHub.Extensions;
using Xunit;

public class UriExtensionTests
{
    public class TheAppendMethod : TestBaseClass
    {
        [Theory]
        [InlineData("https://github.com/foo/bar", "graphs", "https://github.com/foo/bar/graphs")]
        [InlineData("https://github.com/foo/bar/", "graphs", "https://github.com/foo/bar/graphs")]
        [InlineData("https://github.com", "bippety/boppety", "https://github.com/bippety/boppety")]
        [InlineData("https://github.com/", "bippety/boppety", "https://github.com/bippety/boppety")]
        [InlineData("https://github.com/foo/bar", "bippety/boppety", "https://github.com/foo/bar/bippety/boppety")]
        public void AppendsRelativePath(string url, string relativePath, string expected)
        {
            var uri = new Uri(url, UriKind.Absolute);
            var expectedUri = new Uri(expected, UriKind.Absolute);

            var result = uri.Append(relativePath);

            Assert.Equal(expectedUri, result);
        }
    }
}
