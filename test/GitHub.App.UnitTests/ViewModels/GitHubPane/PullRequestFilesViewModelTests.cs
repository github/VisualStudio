using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels.GitHubPane;
using NSubstitute;
using NUnit.Framework;

namespace UnitTests.GitHub.App.ViewModels.GitHubPane
{
    public class PullRequestFilesViewModelTests
    {
        [Test]
        public async Task ShouldCreateChangesTreeAsync()
        {
            var target = CreateTarget();
            var session = CreateSession();

            session.PullRequest.ChangedFiles = new[]
            {
                new PullRequestFileModel { FileName = "readme.md", Sha = "abc", Status = PullRequestFileStatus.Modified },
                new PullRequestFileModel { FileName = "dir1/f1.cs", Sha = "abc", Status = PullRequestFileStatus.Modified },
                new PullRequestFileModel { FileName = "dir1/f2.cs", Sha = "abc", Status = PullRequestFileStatus.Modified },
                new PullRequestFileModel { FileName = "dir1/dir1a/f3.cs", Sha = "abc", Status = PullRequestFileStatus.Modified },
                new PullRequestFileModel { FileName = "dir2/f4.cs", Sha = "abc", Status = PullRequestFileStatus.Modified },
            };

            await target.InitializeAsync(session);

            Assert.That(target.Items.Count, Is.EqualTo(3));

            var dir1 = (PullRequestDirectoryNode)target.Items[0];
            Assert.That(dir1.DirectoryName, Is.EqualTo("dir1"));
            Assert.That(dir1.Files, Has.Exactly(2).Items);

            Assert.That(dir1.Directories, Has.One.Items);
            Assert.That(dir1.Files[0].FileName, Is.EqualTo("f1.cs"));
            Assert.That(dir1.Files[1].FileName, Is.EqualTo("f2.cs"));
            Assert.That(dir1.Files[0].RelativePath, Is.EqualTo("dir1\\f1.cs"));
            Assert.That(dir1.Files[1].RelativePath, Is.EqualTo("dir1\\f2.cs"));

            var dir1a = (PullRequestDirectoryNode)dir1.Directories[0];
            Assert.That(dir1a.DirectoryName, Is.EqualTo("dir1a"));
            Assert.That(dir1a.Files, Has.One.Items);
            Assert.That(dir1a.Directories, Is.Empty);

            var dir2 = (PullRequestDirectoryNode)target.Items[1];
            Assert.That(dir2.DirectoryName, Is.EqualTo("dir2"));
            Assert.That(dir2.Files, Has.One.Items);
            Assert.That(dir2.Directories, Is.Empty);

            var readme = (PullRequestFileNode)target.Items[2];
            Assert.That(readme.FileName, Is.EqualTo("readme.md"));
        }

        [Test]
        public async Task FileCommentCountShouldTrackSessionInlineCommentsAsync()
        {
            var outdatedThread = CreateThread(-1);
            var session = CreateSession();

            session.PullRequest.ChangedFiles = new[]
            {
                new PullRequestFileModel { FileName = "readme.md", Sha = "abc", Status = PullRequestFileStatus.Modified, }
            };

            var file = Substitute.For<IPullRequestSessionFile>();
            var thread1 = CreateThread(5);
            var thread2 = CreateThread(6);
            file.InlineCommentThreads.Returns(new[] { thread1 });
            session.GetFile("readme.md").Returns(Task.FromResult(file));

            var target = CreateTarget();

            await target.InitializeAsync(session);
            Assert.That(((IPullRequestFileNode)target.Items[0]).CommentCount, Is.EqualTo(1));

            file.InlineCommentThreads.Returns(new[] { thread1, thread2 });
            RaisePropertyChanged(file, nameof(file.InlineCommentThreads));
            Assert.That(((IPullRequestFileNode)target.Items[0]).CommentCount, Is.EqualTo(2));

            // Outdated comment is not included in the count.
            file.InlineCommentThreads.Returns(new[] { thread1, thread2, outdatedThread });
            RaisePropertyChanged(file, nameof(file.InlineCommentThreads));
            Assert.That(((IPullRequestFileNode)target.Items[0]).CommentCount, Is.EqualTo(2));

            file.Received(2).PropertyChanged += Arg.Any<PropertyChangedEventHandler>();
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

            var repository = new LocalRepositoryModel { LocalPath = @"C:\Foo" };

            var result = Substitute.For<IPullRequestSession>();
            result.LocalRepository.Returns(repository);
            result.PullRequest.Returns(new PullRequestDetailModel());
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
