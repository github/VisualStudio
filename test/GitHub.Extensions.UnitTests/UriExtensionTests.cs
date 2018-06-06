using System;
using GitHub.Extensions;
using NUnit.Framework;

public class UriExtensionTests
{
    public class TheAppendMethod : TestBaseClass
    {
        [TestCase("https://github.com/foo/bar", "graphs", "https://github.com/foo/bar/graphs")]
        [TestCase("https://github.com/foo/bar/", "graphs", "https://github.com/foo/bar/graphs")]
        [TestCase("https://github.com", "bippety/boppety", "https://github.com/bippety/boppety")]
        [TestCase("https://github.com/", "bippety/boppety", "https://github.com/bippety/boppety")]
        [TestCase("https://github.com/foo/bar", "bippety/boppety", "https://github.com/foo/bar/bippety/boppety")]
        public void AppendsRelativePath(string url, string relativePath, string expected)
        {
            var uri = new Uri(url, UriKind.Absolute);
            var expectedUri = new Uri(expected, UriKind.Absolute);

            var result = uri.Append(relativePath);

            Assert.That(expectedUri, Is.EqualTo(result));
        }
    }
}
