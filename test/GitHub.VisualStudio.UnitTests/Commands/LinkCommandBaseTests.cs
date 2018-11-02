using System;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Exports;
using GitHub.Services;
using GitHub.VisualStudio.Commands;
using NSubstitute;
using NUnit.Framework;

public class LinkCommandBaseTests : TestBaseClass
{
    [TestCase(1, LinkType.Blob, false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs")]
    [TestCase(2, LinkType.Blob, false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, -1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1")]
    [TestCase(3, LinkType.Blob, false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, 1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1")]
    [TestCase(4, LinkType.Blob, false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, 2, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1-L2")]
    [TestCase(5, LinkType.Blob, false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 2, 1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1-L2")]
    [TestCase(6, LinkType.Blob, false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, 2, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs")]
    [TestCase(7, LinkType.Blob, false, "https://github.com/foo/bar", "123123", "", 1, 2, "https://github.com/foo/bar/commit/123123")]
    [TestCase(8, LinkType.Blob, false, "https://github.com/foo/bar", "", @"src\dir\file1.cs", -1, 2, "https://github.com/foo/bar")]
    [TestCase(9, LinkType.Blob, false, "https://github.com/foo/bar", null, null, -1, -1, "https://github.com/foo/bar")]
    [TestCase(10, LinkType.Blob, false, null, "123123", @"src\dir\file1.cs", 1, 2, null)]
    [TestCase(11, LinkType.Blob, true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs")]
    [TestCase(12, LinkType.Blob, true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, -1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1")]
    [TestCase(13, LinkType.Blob, true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, 1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1")]
    [TestCase(14, LinkType.Blob, true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, 2, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1-L2")]
    [TestCase(15, LinkType.Blob, true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 2, 1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1-L2")]
    [TestCase(16, LinkType.Blob, true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, 2, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs")]
    [TestCase(17, LinkType.Blob, true, "https://github.com/foo/bar", "", @"src\dir\file1.cs", -1, 2, "https://github.com/foo/bar")]
    [TestCase(18, LinkType.Blob, true, null, "123123", @"src\dir\file1.cs", 1, 2, null)]
    [TestCase(19, LinkType.Blob, false, "git@github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs")]
    [TestCase(20, LinkType.Blob, false, "git@github.com/foo/bar", "123123", @"src\dir\File1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/File1.cs")]
    [TestCase(21, LinkType.Blob, false, "git@github.com/foo/bar", "123123", @"src\dir\ThisIsFile1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/ThisIsFile1.cs")]
    [TestCase(22, LinkType.Blob, false, "git@github.com/foo/bar", "123123", @"src\dir\ThisIsFile1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/ThisIsFile1.cs")]
    [TestCase(23, LinkType.Blob, false, "git@github.com/foo/bar", "123123", @"src\dir\ThisIsFile1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/ThisIsFile1.cs")]
    [TestCase(24, LinkType.Blob, false, "git@github.com/foo/bar", "123123", @"src\dir\ThisIsFile1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/ThisIsFile1.cs")]
    [TestCase(25, LinkType.Blob, false, "git@github.com/foo/bar", "123123", @"src\dir\ThisIsFile1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/ThisIsFile1.cs")]
    [TestCase(22, LinkType.Blame, true, "git@github.com/foo/bar", "123123", @"src\dir\ThisIsFile1.cs", -1, -1, "https://github.com/foo/bar/blame/123123/src/dir/ThisIsFile1.cs")]
    [TestCase(23, LinkType.Blame, true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, -1, "https://github.com/foo/bar/blame/123123/src/dir/file1.cs")]
    [TestCase(24, LinkType.Blame, false, "https://github.com/foo/bar", "123123", "", 1, 2, "https://github.com/foo/bar/commit/123123")]
    public async Task GenerateUrl(int testid, LinkType linkType, bool createRootedPath, string baseUrl, string sha, string path, int startLine, int endLine, string expected)
    {
        using (var temp = new TempDirectory())
        {
            var gitService = CreateGitService(sha);

            var basePath = temp.Directory.CreateSubdirectory("generate-url-test1-" + testid);
            if (createRootedPath && path != null)
                path = System.IO.Path.Combine(basePath.FullName, path);
            LocalRepositoryModel model = null;
            model = new LocalRepositoryModel { Name = "bar", CloneUrl = baseUrl, LocalPath = basePath.FullName };

            var result = await LinkCommandBase.GenerateUrl(gitService, model, linkType, path, startLine, endLine);

            Assert.That(result?.ToString(), Is.EqualTo(expected));
        }
    }

    static IGitService CreateGitService(string sha)
    {
        var gitservice = Substitute.For<IGitService>();
        gitservice.GetLatestPushedSha(Args.String).Returns(Task.FromResult(sha));
        return gitservice;
    }
}
