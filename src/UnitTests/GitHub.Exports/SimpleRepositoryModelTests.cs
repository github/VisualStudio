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
    void SetupRepository(string sha)
    {
        var provider = Substitutes.ServiceProvider;
        Services.PackageServiceProvider = provider;
        var gitservice = provider.GetGitService();
        var repo = Substitute.For<IRepository>();
        gitservice.GetRepo(Args.String).Returns(repo);
        if (!String.IsNullOrEmpty(sha))
        {
            var commit = Substitute.For<Commit>();
            commit.Sha.Returns(sha);
            repo.Commits.Returns(new FakeCommitLog { commit });
        }
    }

    [Theory]
    [InlineData(false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs")]
    [InlineData(false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, -1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1")]
    [InlineData(false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, 1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1")]
    [InlineData(false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, 2, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1-L2")]
    [InlineData(false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 2, 1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1-L2")]
    [InlineData(false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, 2, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs")]
    [InlineData(false, "https://github.com/foo/bar", "123123", "", 1, 2, "https://github.com/foo/bar/commit/123123")]
    [InlineData(false, "https://github.com/foo/bar", "", @"src\dir\file1.cs", -1, 2, "https://github.com/foo/bar")]
    [InlineData(false, "https://github.com/foo/bar", null, null, -1, -1, "https://github.com/foo/bar")]
    [InlineData(false, null, "123123", @"src\dir\file1.cs", 1, 2, null)]
    [InlineData(true,  "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs")]
    [InlineData(true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, -1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1")]
    [InlineData(true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, 1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1")]
    [InlineData(true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, 2, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1-L2")]
    [InlineData(true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 2, 1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1-L2")]
    [InlineData(true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, 2, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs")]
    [InlineData(true, "https://github.com/foo/bar", "", @"src\dir\file1.cs", -1, 2, "https://github.com/foo/bar")]
    [InlineData(true, null, "123123", @"src\dir\file1.cs", 1, 2, null)]
    [InlineData(false, "git@github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs")]
    public void GenerateUrl(bool createRootedPath, string baseUrl, string sha, string path, int startLine, int endLine, string expected)
    {
        SetupRepository(sha);

        var basePath = Directory.CreateSubdirectory("generate-url-test1");
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
