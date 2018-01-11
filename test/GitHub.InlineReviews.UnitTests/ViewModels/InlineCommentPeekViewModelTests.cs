using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
using Microsoft.VisualStudio.Text.Editor;
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
            var peekSession = CreatePeekSession();
            var target = new InlineCommentPeekViewModel(
                CreatePeekService(lineNumber: 8),
                peekSession,
                sessionManager,
                Substitute.For<INextInlineCommentCommand>(),
                Substitute.For<IPreviousInlineCommentCommand>());

            await target.Initialize();
            Assert.IsType<NewInlineCommentThreadViewModel>(target.Thread);

            target.Thread.Comments[0].Body = "New Comment";

            sessionManager.CurrentSession
                .When(x => x.PostReviewComment(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<IReadOnlyList<DiffChunk>>(),
                    Arg.Any<int>()))
                .Do(async x =>
                {
                    // Simulate the thread being added to the session.
                    var file = await sessionManager.GetLiveFile(
                        RelativePath,
                        peekSession.TextView,
                        peekSession.TextView.TextBuffer);
                    var newThread = CreateThread(8, "New Comment");
                    file.InlineCommentThreads.Returns(new[] { newThread });
                    RaiseLinesChanged(file, Tuple.Create(8, DiffSide.Right));
                });

            await target.Thread.Comments[0].CommitEdit.ExecuteAsyncTask(null);

            Assert.IsType<InlineCommentThreadViewModel>(target.Thread);
        }

        [Fact]
        public async Task RefreshesWhenSessionInlineCommentThreadsChanges()
        {
            var sessionManager = CreateSessionManager();
            var peekSession = CreatePeekSession();
            var target = new InlineCommentPeekViewModel(
                CreatePeekService(lineNumber: 10),
                peekSession,
                sessionManager,
                Substitute.For<INextInlineCommentCommand>(),
                Substitute.For<IPreviousInlineCommentCommand>());

            await target.Initialize();

            Assert.IsType<InlineCommentThreadViewModel>(target.Thread);
            Assert.Equal(2, target.Thread.Comments.Count);

            var file = await sessionManager.GetLiveFile(
                RelativePath,
                peekSession.TextView,
                peekSession.TextView.TextBuffer);
            AddCommentToExistingThread(file);

            Assert.Equal(3, target.Thread.Comments.Count);
        }

        [Fact]
        public async Task RetainsCommentBeingEditedWhenSessionRefreshed()
        {
            var sessionManager = CreateSessionManager();
            var peekSession = CreatePeekSession();
            var target = new InlineCommentPeekViewModel(
                CreatePeekService(lineNumber: 10),
                CreatePeekSession(),
                sessionManager,
                Substitute.For<INextInlineCommentCommand>(),
                Substitute.For<IPreviousInlineCommentCommand>());

            await target.Initialize();

            Assert.Equal(2, target.Thread.Comments.Count);

            var placeholder = target.Thread.Comments.Last();
            placeholder.BeginEdit.Execute(null);
            placeholder.Body = "Comment being edited";

            var file = await sessionManager.GetLiveFile(
                RelativePath,
                peekSession.TextView,
                peekSession.TextView.TextBuffer);
            AddCommentToExistingThread(file);

            placeholder = target.Thread.Comments.Last();
            Assert.Equal(3, target.Thread.Comments.Count);
            Assert.Equal(CommentEditState.Editing, placeholder.EditState);
            Assert.Equal("Comment being edited", placeholder.Body);
        }

        [Fact]
        public async Task DoesntRetainSubmittedCommentInPlaceholderAfterPost()
        {
            var sessionManager = CreateSessionManager();
            var peekSession = CreatePeekSession();
            var target = new InlineCommentPeekViewModel(
                CreatePeekService(lineNumber: 10),
                peekSession,
                sessionManager,
                Substitute.For<INextInlineCommentCommand>(),
                Substitute.For<IPreviousInlineCommentCommand>());

            await target.Initialize();

            Assert.Equal(2, target.Thread.Comments.Count);

            sessionManager.CurrentSession.PostReviewComment(null, 0)
                .ReturnsForAnyArgs(async x =>
                {
                    var file = await sessionManager.GetLiveFile(
                        RelativePath,
                        peekSession.TextView,
                        peekSession.TextView.TextBuffer);
                    AddCommentToExistingThread(file);
                    return file.InlineCommentThreads[0].Comments.Last();
                });

            var placeholder = target.Thread.Comments.Last();
            placeholder.BeginEdit.Execute(null);
            placeholder.Body = "Comment being edited";
            placeholder.CommitEdit.Execute(null);

            placeholder = target.Thread.Comments.Last();
            Assert.Equal(CommentEditState.Placeholder, placeholder.EditState);
            Assert.Equal(string.Empty, placeholder.Body);
        }

        void AddCommentToExistingThread(IPullRequestSessionFile file)
        {
            var newThreads = file.InlineCommentThreads.ToList();
            var thread = file.InlineCommentThreads.Single();
            var newComment = CreateComment("New Comment");
            var newComments = thread.Comments.Concat(new[] { newComment }).ToList();
            thread.Comments.Returns(newComments);
            file.InlineCommentThreads.Returns(newThreads);
            RaiseLinesChanged(file, Tuple.Create(thread.LineNumber, DiffSide.Right));
        }

        IApiClientFactory CreateApiClientFactory()
        {
            var apiClient = Substitute.For<IApiClient>();
            apiClient.CreatePullRequestReviewComment(null, null, 0, null, 0)
                .ReturnsForAnyArgs(_ => Observable.Return(new PullRequestReviewComment()));
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
            result.GetLineNumber(null, null).ReturnsForAnyArgs(Tuple.Create(lineNumber, false));
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
            file.LinesChanged.Returns(new Subject<IReadOnlyList<Tuple<int, DiffSide>>>());

            var session = Substitute.For<IPullRequestSession>();
            session.LocalRepository.CloneUrl.Returns(new UriString("https://foo.bar"));

            var result = Substitute.For<IPullRequestSessionManager>();
            result.CurrentSession.Returns(session);
            result.GetLiveFile(RelativePath, Arg.Any<ITextView>(), Arg.Any<ITextBuffer>()).Returns(file);
            result.GetRelativePath(Arg.Any<ITextBuffer>()).Returns(RelativePath);

            return result;
        }

        void RaiseLinesChanged(IPullRequestSessionFile file, params Tuple<int, DiffSide>[] lineNumbers)
        {
            var subject = (Subject<IReadOnlyList<Tuple<int, DiffSide>>>)file.LinesChanged;
            subject.OnNext(lineNumbers);
        }

        void RaisePropertyChanged<T>(T o, string propertyName)
            where T : INotifyPropertyChanged
        {
            o.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new PropertyChangedEventArgs(propertyName));
        }
    }
}
