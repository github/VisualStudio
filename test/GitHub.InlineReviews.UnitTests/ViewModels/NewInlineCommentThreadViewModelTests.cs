using System;
using System.ComponentModel;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;
using GitHub.Services;
using NSubstitute;
using Octokit;
using NUnit.Framework;
using System.Collections.Generic;

namespace GitHub.InlineReviews.UnitTests.ViewModels
{
    public class NewInlineCommentThreadViewModelTests
    {
        [Test]
        public void CreatesReplyPlaceholder()
        {
            var target = new NewInlineCommentThreadViewModel(
                Substitute.For<IPullRequestSession>(),
                Substitute.For<IPullRequestSessionFile>(),
                10,
                false);

            Assert.That(target.Comments, Has.One.Items);
            Assert.That(string.Empty, Is.EqualTo(target.Comments[0].Body));
            Assert.That(CommentEditState.Editing, Is.EqualTo(target.Comments[0].EditState));
        }

        [Test]
        public void NeedsPushTracksFileCommitSha()
        {
            var file = CreateFile();
            var target = new NewInlineCommentThreadViewModel(
                Substitute.For<IPullRequestSession>(),
                file,
                10,
                false);

            Assert.That(target.NeedsPush, Is.False);
            Assert.That(target.PostComment.CanExecute(false), Is.True);

            file.CommitSha.Returns((string)null);
            RaisePropertyChanged(file, nameof(file.CommitSha));
            Assert.That(target.NeedsPush, Is.True);
            Assert.That(target.PostComment.CanExecute(false), Is.False);

            file.CommitSha.Returns("COMMIT_SHA");
            RaisePropertyChanged(file, nameof(file.CommitSha));
            Assert.That(target.NeedsPush, Is.False);
            Assert.That(target.PostComment.CanExecute(false), Is.True);
        }

        [Test]
        public void PlaceholderCommitEnabledWhenCommentHasBodyAndPostCommentIsEnabled()
        {
            var file = CreateFile();
            var target = new NewInlineCommentThreadViewModel(
                Substitute.For<IPullRequestSession>(),
                file,
                10,
                false);

            file.CommitSha.Returns((string)null);
            RaisePropertyChanged(file, nameof(file.CommitSha));
            Assert.That(target.Comments[0].CommitEdit.CanExecute(null), Is.False);

            target.Comments[0].Body = "Foo";
            Assert.That(target.Comments[0].CommitEdit.CanExecute(null), Is.False);

            file.CommitSha.Returns("COMMIT_SHA");
            RaisePropertyChanged(file, nameof(file.CommitSha));
            Assert.That(target.Comments[0].CommitEdit.CanExecute(null), Is.True);
        }

        [Test]
        public void PostsCommentToCorrectAddedLine()
        {
            var session = CreateSession();
            var file = CreateFile();
            var target = new NewInlineCommentThreadViewModel(session, file, 10, false);

            target.Comments[0].Body = "New Comment";
            target.Comments[0].CommitEdit.Execute(null);

            session.Received(1).PostReviewComment(
                "New Comment",
                "COMMIT_SHA",
                "file.cs",
                Arg.Any<IReadOnlyList<DiffChunk>>(),
                5);
        }

        [Test]
        public void AddsCommentToCorrectDeletedLine()
        {
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

            var target = new NewInlineCommentThreadViewModel(session, file, 16, true);

            target.Comments[0].Body = "New Comment";
            target.Comments[0].CommitEdit.Execute(null);

            session.Received(1).PostReviewComment(
                "New Comment",
                "COMMIT_SHA",
                "file.cs",
                Arg.Any<IReadOnlyList<DiffChunk>>(),
                7);
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
            result.RepositoryOwner.Returns("owner");
            result.LocalRepository.Name.Returns("repo");
            result.LocalRepository.Owner.Returns("shouldnt-be-used");
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
