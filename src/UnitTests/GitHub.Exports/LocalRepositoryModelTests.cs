using System;
using GitHub.Models;
using GitHub.VisualStudio;
using LibGit2Sharp;
using NSubstitute;
using UnitTests;
using Xunit;
using GitHub.Primitives;
using Xunit.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Exports;

[Collection("PackageServiceProvider global data tests")]
public class LocalRepositoryModelTests : TestBaseClass
{
    ITestOutputHelper output;

    public LocalRepositoryModelTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    static void SetupRepository(string sha)
    {
        var provider = Substitutes.ServiceProvider;
        var gitservice = provider.GetGitService();
        var repo = Substitute.For<IRepository>();
        gitservice.GetRepository(Args.String).Returns(repo);
        gitservice.GetLatestPushedSha(Args.String).Returns(Task.FromResult(sha));
        if (!String.IsNullOrEmpty(sha))
        {
            var refs = Substitute.For<ReferenceCollection>();
            var refrence = Substitute.For<Reference>();
            refs.ReachableFrom(Arg.Any<IEnumerable<Reference>>(), Arg.Any<IEnumerable<Commit>>()).Returns(new Reference[] { refrence });
            repo.Refs.Returns(refs);
            var commit = Substitute.For<Commit>();
            commit.Sha.Returns(sha);
            repo.Commits.Returns(new FakeCommitLog { commit });
        }
    }

    [Theory]
    [InlineData(1, LinkType.Blob, false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs")]
    [InlineData(2, LinkType.Blob, false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, -1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1")]
    [InlineData(3, LinkType.Blob, false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, 1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1")]
    [InlineData(4, LinkType.Blob, false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, 2, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1-L2")]
    [InlineData(5, LinkType.Blob, false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 2, 1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1-L2")]
    [InlineData(6, LinkType.Blob, false, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, 2, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs")]
    [InlineData(7, LinkType.Blob, false, "https://github.com/foo/bar", "123123", "", 1, 2, "https://github.com/foo/bar/commit/123123")]
    [InlineData(8, LinkType.Blob, false, "https://github.com/foo/bar", "", @"src\dir\file1.cs", -1, 2, "https://github.com/foo/bar")]
    [InlineData(9, LinkType.Blob, false, "https://github.com/foo/bar", null, null, -1, -1, "https://github.com/foo/bar")]
    [InlineData(10, LinkType.Blob, false, null, "123123", @"src\dir\file1.cs", 1, 2, null)]
    [InlineData(11, LinkType.Blob, true,  "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs")]
    [InlineData(12, LinkType.Blob, true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, -1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1")]
    [InlineData(13, LinkType.Blob, true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, 1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1")]
    [InlineData(14, LinkType.Blob, true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 1, 2, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1-L2")]
    [InlineData(15, LinkType.Blob, true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", 2, 1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs#L1-L2")]
    [InlineData(16, LinkType.Blob, true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, 2, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs")]
    [InlineData(17, LinkType.Blob, true, "https://github.com/foo/bar", "", @"src\dir\file1.cs", -1, 2, "https://github.com/foo/bar")]
    [InlineData(18, LinkType.Blob, true, null, "123123", @"src\dir\file1.cs", 1, 2, null)]
    [InlineData(19, LinkType.Blob, false, "git@github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/file1.cs")]
    [InlineData(20, LinkType.Blob, false, "git@github.com/foo/bar", "123123", @"src\dir\File1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/File1.cs")]
    [InlineData(21, LinkType.Blob, false, "git@github.com/foo/bar", "123123", @"src\dir\ThisIsFile1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/ThisIsFile1.cs")]
    [InlineData(22, LinkType.Blob, false, "git@github.com/foo/bar", "123123", @"src\dir\ThisIsFile1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/ThisIsFile1.cs")]
    [InlineData(23, LinkType.Blob, false, "git@github.com/foo/bar", "123123", @"src\dir\ThisIsFile1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/ThisIsFile1.cs")]
    [InlineData(24, LinkType.Blob, false, "git@github.com/foo/bar", "123123", @"src\dir\ThisIsFile1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/ThisIsFile1.cs")]
    [InlineData(25, LinkType.Blob, false, "git@github.com/foo/bar", "123123", @"src\dir\ThisIsFile1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/ThisIsFile1.cs")]
    [InlineData(22, LinkType.Blame, true, "git@github.com/foo/bar", "123123", @"src\dir\ThisIsFile1.cs", -1, -1, "https://github.com/foo/bar/blame/123123/src/dir/ThisIsFile1.cs")]
    [InlineData(23, LinkType.Blame, true, "https://github.com/foo/bar", "123123", @"src\dir\file1.cs", -1, -1, "https://github.com/foo/bar/blame/123123/src/dir/file1.cs")]
    [InlineData(24, LinkType.Blame, false, "https://github.com/foo/bar", "123123", "", 1, 2, "https://github.com/foo/bar/commit/123123")]
    [InlineData(25, null, false, "git@github.com/foo/bar", "123123", @"src\dir\ThisIsFile1.cs", -1, -1, "https://github.com/foo/bar/blob/123123/src/dir/ThisIsFile1.cs")]

    public async void GenerateUrl(int testid, LinkType linkType, bool createRootedPath, string baseUrl, string sha, string path, int startLine, int endLine, string expected)
    {
        using (var temp = new TempDirectory())
        {
            SetupRepository(sha);

            var basePath = temp.Directory.CreateSubdirectory("generate-url-test1-" + testid);
            if (createRootedPath && path != null)
                path = System.IO.Path.Combine(basePath.FullName, path);
            ILocalRepositoryModel model = null;
            if (!String.IsNullOrEmpty(baseUrl))
                model = new LocalRepositoryModel("bar", new UriString(baseUrl), basePath.FullName);
            else
                model = new LocalRepositoryModel(basePath.FullName);
            var result = await model.GenerateUrl(linkType, path, startLine, endLine);
            Assert.Equal(expected, result?.ToString());
        }
    }
}
