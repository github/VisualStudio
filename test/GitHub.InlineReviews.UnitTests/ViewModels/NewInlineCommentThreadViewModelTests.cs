using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;
using GitHub.Services;
using NSubstitute;
using Octokit;
using Xunit;

namespace GitHub.InlineReviews.UnitTests.ViewModels
{
    public class NewInlineCommentThreadViewModelTests
    {
        [Fact]
        public void CreatesReplyPlaceholder()
        {
            var target = new NewInlineCommentThreadViewModel(
                Substitute.For<IApiClient>(),
                Substitute.For<IPullRequestSession>(),
                Substitute.For<IPullRequestSessionFile>(),
                10,
                false);

            Assert.Equal(1, target.Comments.Count);
            Assert.Equal(string.Empty, target.Comments[0].Body);
            Assert.Equal(CommentEditState.Editing, target.Comments[0].EditState);
        }

        [Fact]
        public void NeedsPushTracksFileCommitSha()
        {
            var file = CreateFile();
            var target = new NewInlineCommentThreadViewModel(
                Substitute.For<IApiClient>(),
                Substitute.For<IPullRequestSession>(),
                file,
                10,
                false);

            Assert.False(target.NeedsPush);
            Assert.True(target.PostComment.CanExecute(false));

            file.CommitSha.Returns((string)null);
            RaisePropertyChanged(file, nameof(file.CommitSha));
            Assert.True(target.NeedsPush);
            Assert.False(target.PostComment.CanExecute(false));

            file.CommitSha.Returns("COMMIT_SHA");
            RaisePropertyChanged(file, nameof(file.CommitSha));
            Assert.False(target.NeedsPush);
            Assert.True(target.PostComment.CanExecute(false));
        }

        [Fact]
        public void PlaceholderCommitEnabledWhenCommentHasBodyAndPostCommentIsEnabled()
        {
            var file = CreateFile();
            var target = new NewInlineCommentThreadViewModel(
                Substitute.For<IApiClient>(),
                Substitute.For<IPullRequestSession>(),
                file,
                10,
                false);

            file.CommitSha.Returns((string)null);
            RaisePropertyChanged(file, nameof(file.CommitSha));
            Assert.False(target.Comments[0].CommitEdit.CanExecute(null));

            target.Comments[0].Body = "Foo";
            Assert.False(target.Comments[0].CommitEdit.CanExecute(null));

            file.CommitSha.Returns("COMMIT_SHA");
            RaisePropertyChanged(file, nameof(file.CommitSha));
            Assert.True(target.Comments[0].CommitEdit.CanExecute(null));
        }

        [Fact]
        public void PostsCommentToCorrectAddedLine()
        {
            var apiClient = CreateApiClient();
            var session = CreateSession();
            var file = CreateFile();
            var target = new NewInlineCommentThreadViewModel(apiClient, session, file, 10, false);

            target.Comments[0].Body = "New Comment";
            target.Comments[0].CommitEdit.Execute(null);

            apiClient.Received(1).CreatePullRequestReviewComment(
                "owner",
                "repo",
                47,
                "New Comment",
                "COMMIT_SHA",
                "file.cs",
                5);
        }

        [Fact]
        public void AddsCommentToCorrectDeletedLine()
        {
            var apiClient = CreateApiClient();
            var session = CreateSession();
            var file = CreateFile();

            file.Diff.Returns(new[]
            {
                new DiffChunk
                {
                    Lines =
                    {
                        new DiffLine { OldLineNumber = 17, DiffLineNumber = 7 }
                    }
                }
            });

            var target = new NewInlineCommentThreadViewModel(apiClient, session, file, 16, true);

            target.Comments[0].Body = "New Comment";
            target.Comments[0].CommitEdit.Execute(null);

            apiClient.Received(1).CreatePullRequestReviewComment(
                "owner",
                "repo",
                47,
                "New Comment",
                "COMMIT_SHA",
                "file.cs",
                7);
        }

        [Fact]
        public void AddsPostedCommentToSession()
        {
            var apiClient = CreateApiClient();
            var session = CreateSession();
            var file = CreateFile();
            var target = new NewInlineCommentThreadViewModel(apiClient, session, file, 10, false);

            target.Comments[0].Body = "New Comment";
            target.Comments[0].CommitEdit.Execute(null);

            session.Received(1).AddComment(Arg.Any<IPullRequestReviewCommentModel>());
        }

        [Fact]
        public void SignalsFinishedWhenCommentPosted()
        {
            var apiClient = CreateApiClient();
            var session = CreateSession();
            var file = CreateFile();
            var target = new NewInlineCommentThreadViewModel(apiClient, session, file, 10, false);
            var signalled = false;

            target.Finished.Subscribe(_ => signalled = true);
            Assert.False(signalled);

            target.Comments[0].Body = "New Comment";
            target.Comments[0].CommitEdit.Execute(null);

            Assert.True(signalled);
        }

        IApiClient CreateApiClient()
        {
            var result = Substitute.For<IApiClient>();
            result.CreatePullRequestReviewComment(null, null, 0, null, null, null, 0)
                .ReturnsForAnyArgs(_ => Observable.Return(new PullRequestReviewComment()));
            return result;
        }

        IPullRequestSessionFile CreateFile()
        {
            var result = Substitute.For<IPullRequestSessionFile>();
            result.CommitSha.Returns("COMMIT_SHA");
            result.Diff.Returns(new[]
            {
                new DiffChunk
                {
                    Lines =
                    {
                        new DiffLine { NewLineNumber = 11, DiffLineNumber = 5 }
                    }
                }
            });
            result.RelativePath.Returns("file.cs");
            return result;
        }

        IPullRequestSession CreateSession()
        {
            var result = Substitute.For<IPullRequestSession>();
            result.Repository.Owner.Returns("owner");
            result.Repository.Name.Returns("repo");
            result.PullRequest.Number.Returns(47);
            return result;
        }

        void RaisePropertyChanged<T>(T o, string propertyName)
            where T : INotifyPropertyChanged
        {
            o.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new PropertyChangedEventArgs(propertyName));
        }
    }
}
