using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels.GitHubPane;
using NSubstitute;
using Xunit;

namespace UnitTests.GitHub.App.ViewModels.GitHubPane
{
    public class PullRequestFilesViewModelTests
    {
        static readonly Uri Uri = new Uri("http://foo");

        [Fact]
        public async Task ShouldCreateChangesTree()
        {
            var target = CreateTarget();
            var session = CreateSession();

            session.PullRequest.ChangedFiles.Returns(new[]
            {
                    new PullRequestFileModel("readme.md", "abc", PullRequestFileStatus.Modified),
                    new PullRequestFileModel("dir1/f1.cs", "abc", PullRequestFileStatus.Modified),
                    new PullRequestFileModel("dir1/f2.cs", "abc", PullRequestFileStatus.Modified),
                    new PullRequestFileModel("dir1/dir1a/f3.cs", "abc", PullRequestFileStatus.Modified),
                    new PullRequestFileModel("dir2/f4.cs", "abc", PullRequestFileStatus.Modified),
            });

            await target.InitializeAsync(session);

            Assert.Equal(3, target.Items.Count);

            var dir1 = (PullRequestDirectoryNode)target.Items[0];
            Assert.Equal("dir1", dir1.DirectoryName);
            Assert.Equal(2, dir1.Files.Count);
            Assert.Equal(1, dir1.Directories.Count);
            Assert.Equal("f1.cs", dir1.Files[0].FileName);
            Assert.Equal("f2.cs", dir1.Files[1].FileName);
            Assert.Equal("dir1\\f1.cs", dir1.Files[0].RelativePath);
            Assert.Equal("dir1\\f2.cs", dir1.Files[1].RelativePath);

            var dir1a = (PullRequestDirectoryNode)dir1.Directories[0];
            Assert.Equal("dir1a", dir1a.DirectoryName);
            Assert.Equal(1, dir1a.Files.Count);
            Assert.Equal(0, dir1a.Directories.Count);

            var dir2 = (PullRequestDirectoryNode)target.Items[1];
            Assert.Equal("dir2", dir2.DirectoryName);
            Assert.Equal(1, dir2.Files.Count);
            Assert.Equal(0, dir2.Directories.Count);

            var readme = (PullRequestFileNode)target.Items[2];
            Assert.Equal("readme.md", readme.FileName);
        }

        [Fact]
        public async Task FileCommentCountShouldTrackSessionInlineComments()
        {
            var outdatedThread = CreateThread(-1);
            var session = CreateSession();

            session.PullRequest.ChangedFiles.Returns(new[]
            {
                new PullRequestFileModel("readme.md", "abc", PullRequestFileStatus.Modified),
            });

            var file = Substitute.For<IPullRequestSessionFile>();
            var thread1 = CreateThread(5);
            var thread2 = CreateThread(6);
            file.InlineCommentThreads.Returns(new[] { thread1 });
            session.GetFile("readme.md").Returns(Task.FromResult(file));

            var target = CreateTarget();

            await target.InitializeAsync(session);
            Assert.Equal(1, ((IPullRequestFileNode)target.Items[0]).CommentCount);

            file.InlineCommentThreads.Returns(new[] { thread1, thread2 });
            RaisePropertyChanged(file, nameof(file.InlineCommentThreads));
            Assert.Equal(2, ((IPullRequestFileNode)target.Items[0]).CommentCount);

            // Outdated comment is not included in the count.
            file.InlineCommentThreads.Returns(new[] { thread1, thread2, outdatedThread });
            RaisePropertyChanged(file, nameof(file.InlineCommentThreads));
            Assert.Equal(2, ((IPullRequestFileNode)target.Items[0]).CommentCount);

            file.Received(1).PropertyChanged += Arg.Any<PropertyChangedEventHandler>();
        }

        static PullRequestFilesViewModel CreateTarget()
        {
            var pullRequestService = Substitute.For<IPullRequestService>();
            var editorService = Substitute.For<IPullRequestEditorService>();
            return new PullRequestFilesViewModel(pullRequestService, editorService);
        }

        static IPullRequestSession CreateSession()
        {
            var author = Substitute.For<IAccount>();
            var pr = Substitute.For<IPullRequestModel>();

            var repository = Substitute.For<ILocalRepositoryModel>();
            repository.LocalPath.Returns(@"C:\Foo");

            var result = Substitute.For<IPullRequestSession>();
            result.LocalRepository.Returns(repository);
            result.PullRequest.Returns(pr);
            return result;
        }

        IInlineCommentThreadModel CreateThread(int lineNumber)
        {
            var result = Substitute.For<IInlineCommentThreadModel>();
            result.LineNumber.Returns(lineNumber);
            return result;
        }

        void RaisePropertyChanged<T>(T o, string propertyName)
            where T : INotifyPropertyChanged
        {
            o.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new PropertyChangedEventArgs(propertyName));
        }
    }
}
