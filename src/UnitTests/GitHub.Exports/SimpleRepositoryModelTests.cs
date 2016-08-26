using System;
using GitHub.Models;
using GitHub.VisualStudio;
using LibGit2Sharp;
using NSubstitute;
using UnitTests;
using Xunit;
using GitHub.Primitives;

[Collection("PackageServiceProvider global data tests")]
public class SimpleRepositoryModelTests : TempFileBaseClass
{
    static void SetupRepository(string sha)
    {
        var provider = Substitutes.ServiceProvider;
        var gitservice = provider.GetGitService();
        var repo = Substitute.For<IRepository>();
        gitservice.GetRepository(Args.String).Returns(repo);
        if (!String.IsNullOrEmpty(sha))
        {
            var commit = Substitute.For<Commit>();
            commit.Sha.Returns(sha);
            repo.Commits.Returns(new FakeCommitLog { commit });
        }
    }

    [Theory]
    [InlineData(1, false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs")]
    [InlineData(2, false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, -1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1")]
    [InlineData(3, false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, 1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1")]
    [InlineData(4, false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, 2, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1-L2")]
    [InlineData(5, false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 2, 1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1-L2")]
    [InlineData(6, false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, 2, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs")]
    [InlineData(7, false, "https://github.com/foo/bar", "123123", "", 1, 2, "https://github.com/foo/bar/commit/123123")]
    [InlineData(8, false, "https://github.com/foo/bar", "", @"src\dir\file1.cs", -1, 2, "https://github.com/foo/bar")]
    [InlineData(9, false, "https://github.com/foo/bar", null, null, -1, -1, "https://github.com/foo/bar")]
    [InlineData(10, false, null, "123123", @"src\dir\file1.cs", 1, 2, null)]
    [InlineData(11, true,  "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs")]
    [InlineData(12, true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, -1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1")]
    [InlineData(13, true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, 1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1")]
    [InlineData(14, true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, 2, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1-L2")]
    [InlineData(15, true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 2, 1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1-L2")]
    [InlineData(16, true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, 2, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs")]
    [InlineData(17, true, "https://github.com/foo/bar", "", @"src\dir\file1.cs", -1, 2, "https://github.com/foo/bar")]
    [InlineData(18, true, null, "123123", @"src\dir\file1.cs", 1, 2, null)]
    [InlineData(19, false, "git@github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs")]
    [InlineData(20, false, "git@github.com/foo/bar", "123123", @"src\dir\File1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/File1.cs")]
    [InlineData(21, false, "git@github.com/foo/bar", "123123", @"src\dir\ThisIsFile1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/ThisIsFile1.cs")]
    public void GenerateUrl(int testid, bool createRootedPath, string baseUrl, string sha, string path, int startLine, int endLine, string expected)
    {
        SetupRepository(sha);

        var basePath = Directory.CreateSubdirectory("generate-url-test1-" + testid);
        if (createRootedPath && path != null)
            path = System.IO.Path.Combine(basePath.FullName, path);
        ISimpleRepositoryModel model = null;
        if (!String.IsNullOrEmpty(baseUrl))
            model = new SimpleRepositoryModel("bar", new UriString(baseUrl), basePath.FullName);
        else
            model = new SimpleRepositoryModel(basePath.FullName);
        var result = model.GenerateUrl(path, startLine, endLine);
        Assert.Equal(expected, result?.ToString());
    }
}
