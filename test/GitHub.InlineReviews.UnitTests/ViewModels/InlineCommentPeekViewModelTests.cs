using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Factories;
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
using NUnit.Framework;
using GitHub.Commands;
using GitHub.ViewModels;
using ReactiveUI.Testing;
using System.Reactive.Concurrency;

namespace GitHub.InlineReviews.UnitTests.ViewModels
{
    public class InlineCommentPeekViewModelTests
    {
        const string FullPath = "c:\\repo\\test.cs";
        const string RelativePath = "test.cs";

        [Test]
        public async Task ThreadIsCreatedForExistingComments()
        {
            // There is an existing comment thread at line 10.
            var target = new InlineCommentPeekViewModel(
                CreatePeekService(lineNumber: 10),
                CreatePeekSession(),
                CreateSessionManager(),
                Substitute.For<INextInlineCommentCommand>(),
                Substitute.For<IPreviousInlineCommentCommand>(),
                CreateFactory());

            await target.Initialize();

            // There should be an existing comment and a reply placeholder.
            Assert.That(target.Thread.IsNewThread, Is.False);
            Assert.That(target.Thread.Comments.Count, Is.EqualTo(2));
            Assert.That(target.Thread.Comments[0].Body, Is.EqualTo("Existing comment"));
            Assert.That(target.Thread.Comments[1].Body, Is.EqualTo(null));
            Assert.That(target.Thread.Comments[1].EditState, Is.EqualTo(CommentEditState.Placeholder));
        }

        [Test]
        public async Task ThreadIsCreatedForNewComment()
        {
            // There is no existing comment thread at line 9, but there is a + diff entry.
            var target = new InlineCommentPeekViewModel(
                CreatePeekService(lineNumber: 9),
                CreatePeekSession(),
                CreateSessionManager(),
                Substitute.For<INextInlineCommentCommand>(),
                Substitute.For<IPreviousInlineCommentCommand>(),
                CreateFactory());

            await target.Initialize();

            Assert.That(target.Thread.IsNewThread, Is.True);
            Assert.That(target.Thread.Comments[0].Body, Is.EqualTo(null));
            Assert.That(target.Thread.Comments[0].EditState, Is.EqualTo(CommentEditState.Editing));
        }

        [Test]
        public async Task ShouldGetRelativePathFromTextBufferInfoIfPresent()
        {
            var session = CreateSession();
            var bufferInfo = new PullRequestTextBufferInfo(session, RelativePath, "123", DiffSide.Right);
            var sessionManager = CreateSessionManager(
                relativePath: "ShouldNotUseThis",
                session: session,
                textBufferInfo: bufferInfo);

            // There is an existing comment thread at line 10.
            var target = new InlineCommentPeekViewModel(
                CreatePeekService(lineNumber: 10),
                CreatePeekSession(),
                sessionManager,
                Substitute.For<INextInlineCommentCommand>(),
                Substitute.For<IPreviousInlineCommentCommand>(),
                CreateFactory());

            await target.Initialize();

            // There should be an existing comment and a reply placeholder.
            Assert.That(target.Thread.IsNewThread, Is.False);
            Assert.That(target.Thread.Comments.Count, Is.EqualTo(2));
            Assert.That(target.Thread.Comments[0].Body, Is.EqualTo("Existing comment"));
            Assert.That(target.Thread.Comments[1].Body, Is.EqualTo(null));
            Assert.That(target.Thread.Comments[1].EditState, Is.EqualTo(CommentEditState.Placeholder));
        }

        [Test]
        public async Task SwitchesFromNewThreadToExistingThreadWhenCommentPosted()
        {
            var sessionManager = CreateSessionManager();
            var peekSession = CreatePeekSession();
            var target = new InlineCommentPeekViewModel(
                CreatePeekService(lineNumber: 8),
                peekSession,
                sessionManager,
                Substitute.For<INextInlineCommentCommand>(),
                Substitute.For<IPreviousInlineCommentCommand>(),
                CreateFactory());

            await target.Initialize();
            Assert.That(target.Thread.IsNewThread, Is.True);

            target.Thread.Comments[0].Body = "New Comment";

            sessionManager.CurrentSession
                .When(x => x.PostReviewComment(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<IReadOnlyList<DiffChunk>>(),
                    Arg.Any<int>()))
                .Do(async _ =>
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

            await target.Thread.Comments[0].CommitEdit.Execute();

            Assert.That(target.Thread.IsNewThread, Is.False);
        }

        [Test]
        public async Task RefreshesWhenSessionInlineCommentThreadsChanges()
        {
            var sessionManager = CreateSessionManager();
            var peekSession = CreatePeekSession();
            var target = new InlineCommentPeekViewModel(
                CreatePeekService(lineNumber: 10),
                peekSession,
                sessionManager,
                Substitute.For<INextInlineCommentCommand>(),
                Substitute.For<IPreviousInlineCommentCommand>(),
                CreateFactory());

            await target.Initialize();

            Assert.That(target.Thread.IsNewThread, Is.False);
            Assert.That(target.Thread.Comments.Count, Is.EqualTo(2));

            var file = await sessionManager.GetLiveFile(
                RelativePath,
                peekSession.TextView,
                peekSession.TextView.TextBuffer);
            AddCommentToExistingThread(file);

            Assert.That(target.Thread.Comments.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task CommittingEditDoesntRetainSubmittedCommentInPlaceholderAfterPost()
        {
            var sessionManager = CreateSessionManager();
            var peekSession = CreatePeekSession();
            var target = new InlineCommentPeekViewModel(
                CreatePeekService(lineNumber: 10),
                peekSession,
                sessionManager,
                Substitute.For<INextInlineCommentCommand>(),
                Substitute.For<IPreviousInlineCommentCommand>(),
                CreateFactory());

            await target.Initialize();

            Assert.That(target.Thread.Comments.Count, Is.EqualTo(2));

            sessionManager.CurrentSession.PostReviewComment(null, null)
                .ReturnsForAnyArgs(async x =>
                {
                    var file = await sessionManager.GetLiveFile(
                        RelativePath,
                        peekSession.TextView,
                        peekSession.TextView.TextBuffer);
                    AddCommentToExistingThread(file);
                });

            var placeholder = target.Thread.Comments.Last();
            await placeholder.BeginEdit.Execute();
            placeholder.Body = "Comment being edited";
            await placeholder.CommitEdit.Execute();

            placeholder = target.Thread.Comments.Last();
            Assert.That(placeholder.EditState, Is.EqualTo(CommentEditState.Placeholder));
            Assert.That(placeholder.Body, Is.EqualTo(null));
        }

        [Test]
        public async Task StartingReviewDoesntRetainSubmittedCommentInPlaceholderAfterPost()
        {
            var sessionManager = CreateSessionManager();
            var peekSession = CreatePeekSession();
            var target = new InlineCommentPeekViewModel(
                CreatePeekService(lineNumber: 10),
                peekSession,
                sessionManager,
                Substitute.For<INextInlineCommentCommand>(),
                Substitute.For<IPreviousInlineCommentCommand>(),
                CreateFactory());

            await target.Initialize();

            Assert.That(target.Thread.Comments.Count, Is.EqualTo(2));

            sessionManager.CurrentSession.StartReview()
                .ReturnsForAnyArgs(async x =>
                {
                    var file = await sessionManager.GetLiveFile(
                        RelativePath,
                        peekSession.TextView,
                        peekSession.TextView.TextBuffer);
                    RaiseLinesChanged(file, Tuple.Create(10, DiffSide.Right));
                });

            var placeholder = (IPullRequestReviewCommentViewModel)target.Thread.Comments.Last();
            await placeholder.BeginEdit.Execute();
            placeholder.Body = "Comment being edited";
            await placeholder.StartReview.Execute();

            placeholder = (IPullRequestReviewCommentViewModel)target.Thread.Comments.Last();
            Assert.That(placeholder.EditState, Is.EqualTo(CommentEditState.Placeholder));
            Assert.That(placeholder.Body, Is.EqualTo(null));
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

        static InlineCommentModel CreateComment(string body)
        {
            return new InlineCommentModel
            {
                Comment = new PullRequestReviewCommentModel
                {
                    Body = body,
                },
                Review = new PullRequestReviewModel(),
            };
        }

        static IViewViewModelFactory CreateFactory()
        {
            var draftStore = Substitute.For<IMessageDraftStore>();
            var commentService = Substitute.For<ICommentService>();
            var result = Substitute.For<IViewViewModelFactory>();
            result.CreateViewModel<IPullRequestReviewCommentViewModel>().Returns(_ =>
                new PullRequestReviewCommentViewModel(commentService));
            result.CreateViewModel<IPullRequestReviewCommentThreadViewModel>().Returns(_ =>
                new PullRequestReviewCommentThreadViewModel(draftStore, result));
            return result;
        }

        static IInlineCommentThreadModel CreateThread(int lineNumber, params string[] comments)
        {
            var result = Substitute.For<IInlineCommentThreadModel>();
            var commentList = comments.Select(x => CreateComment(x)).ToList();
            result.Comments.Returns(commentList);
            result.LineNumber.Returns(lineNumber);
            return result;
        }

        static IInlineCommentPeekService CreatePeekService(int lineNumber)
        {
            var result = Substitute.For<IInlineCommentPeekService>();
            result.GetLineNumber(null, null).ReturnsForAnyArgs(Tuple.Create(lineNumber, false));
            return result;
        }

        static IPeekSession CreatePeekSession()
        {
            var document = Substitute.For<ITextDocument>();
            document.FilePath.Returns(FullPath);

            var propertyCollection = new PropertyCollection();
            propertyCollection.AddProperty(typeof(ITextDocument), document);

            var result = Substitute.For<IPeekSession>();
            result.TextView.TextBuffer.Properties.Returns(propertyCollection);

            return result;
        }

        static IPullRequestSession CreateSession()
        {
            var result = Substitute.For<IPullRequestSession>();
            result.PullRequest.Returns(new PullRequestDetailModel());
            result.User.Returns(new ActorModel { Login = "CurrentUser" });
            result.LocalRepository.Returns(new LocalRepositoryModel { CloneUrl = new UriString("https://foo.bar") });
            return result;
        }

        static IPullRequestSessionManager CreateSessionManager(
            string commitSha = "COMMIT",
            string relativePath = RelativePath,
            IPullRequestSession session = null,
            PullRequestTextBufferInfo textBufferInfo = null)
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

            session = session ?? CreateSession();

            if (textBufferInfo != null)
            {
                session.GetFile(textBufferInfo.RelativePath, textBufferInfo.CommitSha).Returns(file);
            }

            var result = Substitute.For<IPullRequestSessionManager>();
            result.CurrentSession.Returns(session);
            result.GetLiveFile(relativePath, Arg.Any<ITextView>(), Arg.Any<ITextBuffer>()).Returns(file);
            result.GetRelativePath(Arg.Any<ITextBuffer>()).Returns(relativePath);
            result.GetTextBufferInfo(Arg.Any<ITextBuffer>()).Returns(textBufferInfo);

            return result;
        }

        static void RaiseLinesChanged(IPullRequestSessionFile file, params Tuple<int, DiffSide>[] lineNumbers)
        {
            var subject = (Subject<IReadOnlyList<Tuple<int, DiffSide>>>)file.LinesChanged;
            subject.OnNext(lineNumbers);
        }

        static void RaisePropertyChanged<T>(T o, string propertyName)
            where T : INotifyPropertyChanged
        {
            o.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new PropertyChangedEventArgs(propertyName));
        }
    }
}
