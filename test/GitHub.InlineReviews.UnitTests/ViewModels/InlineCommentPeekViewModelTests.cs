using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Factories;
using GitHub.InlineReviews.Commands;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using NSubstitute;
using Octokit;
using ReactiveUI;
using Xunit;

namespace GitHub.InlineReviews.UnitTests.ViewModels
{
    public class InlineCommentPeekViewModelTests
    {
        const string FullPath = "c:\\repo\\test.cs";
        const string RelativePath = "test.cs";

        [Fact]
        public async Task ThreadIsCreatedForExistingComments()
        {
            // There is an existing comment thread at line 10.
            var target = new InlineCommentPeekViewModel(
                Substitute.For<IApiClientFactory>(),
                CreatePeekService(lineNumber: 10),
                CreatePeekSession(),
                CreateSessionManager(),
                Substitute.For<INextInlineCommentCommand>(),
                Substitute.For<IPreviousInlineCommentCommand>());

            await target.Initialize();

            // There should be an existing comment and a reply placeholder.
            Assert.IsType<InlineCommentThreadViewModel>(target.Thread);
            Assert.Equal(2, target.Thread.Comments.Count);
            Assert.Equal("Existing comment", target.Thread.Comments[0].Body);
            Assert.Equal(string.Empty, target.Thread.Comments[1].Body);
            Assert.Equal(CommentEditState.Placeholder, target.Thread.Comments[1].EditState);
        }

        [Fact]
        public async Task ThreadIsCreatedForNewComment()
        {
            // There is no existing comment thread at line 9, but there is a + diff entry.
            var target = new InlineCommentPeekViewModel(
                Substitute.For<IApiClientFactory>(),
                CreatePeekService(lineNumber: 9),
                CreatePeekSession(),
                CreateSessionManager(),
                Substitute.For<INextInlineCommentCommand>(),
                Substitute.For<IPreviousInlineCommentCommand>());

            await target.Initialize();

            Assert.IsType<NewInlineCommentThreadViewModel>(target.Thread);
            Assert.Equal(string.Empty, target.Thread.Comments[0].Body);
            Assert.Equal(CommentEditState.Editing, target.Thread.Comments[0].EditState);
        }

        [Fact]
        public async Task SwitchesFromNewThreadToExistingThreadWhenCommentPosted()
        {
            var sessionManager = CreateSessionManager();
            var target = new InlineCommentPeekViewModel(
                CreateApiClientFactory(),
                CreatePeekService(lineNumber: 8),
                CreatePeekSession(),
                sessionManager,
                Substitute.For<INextInlineCommentCommand>(),
                Substitute.For<IPreviousInlineCommentCommand>());

            await target.Initialize();
            Assert.IsType<NewInlineCommentThreadViewModel>(target.Thread);

            target.Thread.Comments[0].Body = "New Comment";

            sessionManager.CurrentSession
                .When(x => x.AddComment(Arg.Any<IPullRequestReviewCommentModel>()))
                .Do(async x =>
                {
                    // Simulate the thread being added to the session.
                    var file = await sessionManager.CurrentSession.GetFile(RelativePath);
                    var newThread = CreateThread(8, "New Comment");
                    file.InlineCommentThreads.Returns(new[] { newThread });
                });

            await target.Thread.Comments[0].CommitEdit.ExecuteAsyncTask(null);

            Assert.IsType<InlineCommentThreadViewModel>(target.Thread);
        }

        [Fact]
        public async Task RefreshesWhenSessionInlineCommentThreadsChanges()
        {
            var sessionManager = CreateSessionManager();
            var target = new InlineCommentPeekViewModel(
                Substitute.For<IApiClientFactory>(),
                CreatePeekService(lineNumber: 10),
                CreatePeekSession(),
                sessionManager,
                Substitute.For<INextInlineCommentCommand>(),
                Substitute.For<IPreviousInlineCommentCommand>());

            await target.Initialize();

            Assert.IsType<InlineCommentThreadViewModel>(target.Thread);
            Assert.Equal(2, target.Thread.Comments.Count);

            var file = await sessionManager.CurrentSession.GetFile(RelativePath);
            var newThreads = file.InlineCommentThreads.ToList();
            var thread = file.InlineCommentThreads.Single();
            var newComment = CreateComment("New Comment");
            var newComments = thread.Comments.Concat(new[] { newComment }).ToList();
            thread.Comments.Returns(newComments);
            file.InlineCommentThreads.Returns(newThreads);
            RaisePropertyChanged(file, nameof(file.InlineCommentThreads));

            Assert.Equal(3, target.Thread.Comments.Count);
        }

        IApiClientFactory CreateApiClientFactory()
        {
            var apiClient = Substitute.For<IApiClient>();
            apiClient.CreatePullRequestReviewComment(null, null, 0, null, null, null, 0)
                .ReturnsForAnyArgs(_ => Observable.Return(new PullRequestReviewComment()));

            var result = Substitute.For<IApiClientFactory>();
            result.Create(null).ReturnsForAnyArgs(apiClient);
            return result;
        }

        IPullRequestReviewCommentModel CreateComment(string body)
        {
            var comment = Substitute.For<IPullRequestReviewCommentModel>();
            comment.Body.Returns(body);
            return comment;
        }

        IInlineCommentThreadModel CreateThread(int lineNumber, params string[] comments)
        {
            var result = Substitute.For<IInlineCommentThreadModel>();
            var commentList = comments.Select(x => CreateComment(x)).ToList();
            result.Comments.Returns(commentList);
            result.LineNumber.Returns(lineNumber);
            return result;
        }

        IInlineCommentPeekService CreatePeekService(int lineNumber)
        {
            var result = Substitute.For<IInlineCommentPeekService>();
            result.GetLineNumber(null, null).ReturnsForAnyArgs(lineNumber);
            return result;
        }

        IPeekSession CreatePeekSession()
        {
            var document = Substitute.For<ITextDocument>();
            document.FilePath.Returns(FullPath);

            var propertyCollection = new PropertyCollection();
            propertyCollection.AddProperty(typeof(ITextDocument), document);

            var result = Substitute.For<IPeekSession>();
            result.TextView.TextBuffer.Properties.Returns(propertyCollection);

            return result;
        }

        IPullRequestSessionManager CreateSessionManager(string commitSha = "COMMIT")
        {
            var thread = CreateThread(10, "Existing comment");

            var diff = new DiffChunk
            {
                DiffLine = 10,
                OldLineNumber = 1,
                NewLineNumber = 1,
            };

            for (var i = 0; i < 10; ++i)
            {
                diff.Lines.Add(new DiffLine
                {
                    NewLineNumber = i,
                    DiffLineNumber = i + 10,
                    Type = i < 5 ? DiffChangeType.Delete : DiffChangeType.Add,
                });
            }

            var file = Substitute.For<IPullRequestSessionFile>();
            file.CommitSha.Returns(commitSha);
            file.Diff.Returns(new[] { diff });
            file.InlineCommentThreads.Returns(new[] { thread });

            var session = Substitute.For<IPullRequestSession>();
            session.GetFile(RelativePath).Returns(file);
            session.GetRelativePath(FullPath).Returns(RelativePath);
            session.Repository.CloneUrl.Returns(new UriString("https://foo.bar"));

            var result = Substitute.For<IPullRequestSessionManager>();
            result.CurrentSession.Returns(session);

            return result;
        }

        void RaisePropertyChanged<T>(T o, string propertyName)
            where T : INotifyPropertyChanged
        {
            o.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new PropertyChangedEventArgs(propertyName));
        }
    }
}
