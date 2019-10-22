using System;
using GitHub.Services;
using NUnit.Framework;

public static class RepositoryLayoutUtilitiesTests
{
    public class TheGetDefaultPathAndLayoutMethod
    {
        [TestCase(@"c:\source\owner\repositoryName", "https://github.com/owner/repositoryName", @"c:\source", RepositoryLayout.OwnerName)]
        [TestCase(@"c:\source\owner\differentName", "https://github.com/owner/repositoryName", @"c:\source", RepositoryLayout.OwnerName)]
        [TestCase(@"c:\source\repositoryName", "https://github.com/owner/repositoryName", @"c:\source", RepositoryLayout.Name)]
        [TestCase(@"c:\source\differentName", "https://github.com/owner/repositoryName", @"c:\source", RepositoryLayout.Name)]
        public void GetDefaultPathAndLayout(string repositoryPath, string cloneUrl, string expectPath, RepositoryLayout expectLayout)
        {
            var (path, layout) = RepositoryLayoutUtilities.GetDefaultPathAndLayout(repositoryPath, cloneUrl);

            Assert.That((path, layout), Is.EqualTo((expectPath, expectLayout)));
        }
    }
}
