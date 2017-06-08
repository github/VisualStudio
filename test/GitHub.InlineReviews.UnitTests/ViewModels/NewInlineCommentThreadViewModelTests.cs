using System;
using System.ComponentModel;
using GitHub.Api;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;
using GitHub.Services;
using NSubstitute;
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
                10);

            Assert.Equal(1, target.Comments.Count);
            Assert.Equal(string.Empty, target.Comments[0].Body);
            Assert.Equal(CommentEditState.Editing, target.Comments[0].EditState);
        }

        [Fact]
        public void NeedsPushTracksFileCommitSha()
        {
            var file = Substitute.For<IPullRequestSessionFile>();
            file.CommitSha.Returns("COMMIT_SHA");

            var target = new NewInlineCommentThreadViewModel(
                Substitute.For<IApiClient>(),
                Substitute.For<IPullRequestSession>(),
                file,
                10);

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
            var file = Substitute.For<IPullRequestSessionFile>();

            var target = new NewInlineCommentThreadViewModel(
                Substitute.For<IApiClient>(),
                Substitute.For<IPullRequestSession>(),
                file,
                10);

            file.CommitSha.Returns((string)null);
            RaisePropertyChanged(file, nameof(file.CommitSha));
            Assert.False(target.Comments[0].CommitEdit.CanExecute(null));

            target.Comments[0].Body = "Foo";
            Assert.False(target.Comments[0].CommitEdit.CanExecute(null));

            file.CommitSha.Returns("COMMIT_SHA");
            RaisePropertyChanged(file, nameof(file.CommitSha));
            Assert.True(target.Comments[0].CommitEdit.CanExecute(null));
        }

        void RaisePropertyChanged<T>(T o, string propertyName)
            where T : INotifyPropertyChanged
        {
            o.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new PropertyChangedEventArgs(propertyName));
        }
    }
}
