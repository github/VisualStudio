using System;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.ViewModels;
using NSubstitute;
using Octokit;
using Xunit;

namespace UnitTests.GitHub.App.ViewModels
{
    public class PullRequestDetailViewModelTests : TestBaseClass
    {
        static readonly Uri Uri = new Uri("http://foo");

        [Fact]
        public async Task ShouldCreateChangesTree()
        {
            var repository = Substitute.For<ILocalRepositoryModel>();
            repository.CloneUrl.Returns(new UriString(Uri.ToString()));

            var target = new PullRequestDetailViewModel(
                Substitute.For<IRepositoryHost>(),
                repository,
                Substitute.For<IGitService>(),
                Substitute.For<IPullRequestService>(),
                Substitute.For<IAvatarProvider>());

            var files = new[]
            {
                new PullRequestFile(string.Empty, "readme.md", "added", 1, 0, 0, Uri, Uri, Uri, string.Empty),
                new PullRequestFile(string.Empty, "dir1/f1.cs", "added", 1, 0, 0, Uri, Uri, Uri, string.Empty),
                new PullRequestFile(string.Empty, "dir1/f2.cs", "added", 1, 0, 0, Uri, Uri, Uri, string.Empty),
                new PullRequestFile(string.Empty, "dir1/dir1a/f3.cs", "added", 1, 0, 0, Uri, Uri, Uri, string.Empty),
                new PullRequestFile(string.Empty, "dir2/f4.cs", "added", 1, 0, 0, Uri, Uri, Uri, string.Empty),
            };

            await target.Load(CreatePullRequest(), files);

            Assert.Equal(3, target.ChangedFilesTree.Count);

            var dir1 = (PullRequestDirectoryViewModel)target.ChangedFilesTree[0];
            Assert.Equal("dir1", dir1.DirectoryName);
            Assert.Equal(2, dir1.Files.Count);
            Assert.Equal(1, dir1.Directories.Count);

            var dir1a = (PullRequestDirectoryViewModel)dir1.Directories[0];
            Assert.Equal("dir1a", dir1a.DirectoryName);
            Assert.Equal(1, dir1a.Files.Count);
            Assert.Equal(0, dir1a.Directories.Count);

            var dir2 = (PullRequestDirectoryViewModel)target.ChangedFilesTree[1];
            Assert.Equal("dir2", dir2.DirectoryName);
            Assert.Equal(1, dir2.Files.Count);
            Assert.Equal(0, dir2.Directories.Count);

            var readme = (PullRequestFileViewModel)target.ChangedFilesTree[2];
            Assert.Equal("readme.md", readme.FileName);
        }

        PullRequest CreatePullRequest()
        {
            var repository = CreateRepository("foo", "bar");
            var user = CreateUserAndScopes("foo").User;

            return new PullRequest(
                Uri, Uri, Uri, Uri, Uri, Uri,
                1, ItemState.Open, "PR 1", string.Empty,
                DateTimeOffset.Now, DateTimeOffset.Now, null, null,
                new GitReference(string.Empty, "foo:bar", "bar", string.Empty, user, repository),
                new GitReference(string.Empty, "foo:baz", "baz", string.Empty, user, repository),
                user, user,
                true, null,
                0, 0, 0, 0, 0, 0, null, false);
        }
    }
}
