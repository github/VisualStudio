using GitHub.Models;
using GitHub.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Assert.True(a == b);
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
            Assert.True(a == b);
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
}
