using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using GitHub.Factories;
using GitHub.InlineReviews.Models;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.UnitTests.TestDoubles;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using NSubstitute;
using NUnit.Framework;
using System.ComponentModel;
using GitHub.Api;
using System.Reactive.Concurrency;
using ReactiveUI.Testing;

namespace GitHub.InlineReviews.UnitTests.Services
{
    public class PullRequestSessionManagerTests
    {
        const int CurrentBranchPullRequestNumber = 15;
        const int NotCurrentBranchPullRequestNumber = 10;
        const string OwnerCloneUrl = "https://github.com/owner/repo";
        static readonly ActorModel CurrentUser = new ActorModel { Login = "currentUser" };

        public class TheConstructor : PullRequestSessionManagerTests
        {
            [Test]
            public void ReadsPullRequestFromCorrectFork()
            {
                var service = CreatePullRequestService();
                var sessionService = CreateSessionService();

                service.GetPullRequestForCurrentBranch(null).ReturnsForAnyArgs(
                    Observable.Return(("fork", CurrentBranchPullRequestNumber)));

                var connectionManager = CreateConnectionManager();
                var target = CreateTarget(
                    service: service,
                    sessionService: sessionService,
                    connectionManager: connectionManager);

                var address = HostAddress.Create(OwnerCloneUrl);
                sessionService.Received(1).ReadPullRequestDetail(address, "fork", "repo", 15);
            }

            [Test]
            public void LocalRepositoryModelNull()
            {
                var repositoryModel = null as LocalRepositoryModel;
                var teamExplorerContext = CreateTeamExplorerContext(repositoryModel);

                var target = CreateTarget(teamExplorerContext: teamExplorerContext);

                Assert.Null(target.CurrentSession);
            }
        }

        public class TheCurrentSessionProperty : PullRequestSessionManagerTests
        {
            [Test]
            public void CreatesSessionForCurrentBranch()
            {
                var target = CreateTarget();

                Assert.That(target.CurrentSession, Is.Not.Null);
                Assert.That(target.CurrentSession.IsCheckedOut, Is.True);
            }

            [Test]
            public void CurrentSessionIsNullIfNoPullRequestForCurrentBranch()
            {
                var service = CreatePullRequestService();
                service.GetPullRequestForCurrentBranch(null).ReturnsForAnyArgs(Observable.Empty<(string, int)>());

                var target = CreateTarget(service: service);

                Assert.That(target.CurrentSession, Is.Null);
            }

            [Test]
            public void CurrentSessionChangesWhenBranchChanges()
            {
                var service = CreatePullRequestService();
                var teamExplorerContext = CreateTeamExplorerContext(CreateRepositoryModel());
                var target = CreateTarget(
                    service: service,
                    teamExplorerContext: teamExplorerContext);

                var session = target.CurrentSession;

                service.GetPullRequestForCurrentBranch(null).ReturnsForAnyArgs(Observable.Return(("foo", 22)));
                teamExplorerContext.StatusChanged += Raise.Event();

                Assert.That(session, Is.Not.SameAs(target.CurrentSession));
            }

            [Test]
            public void LocalRepositoryModelNull()
            {
                var repositoryModel = null as LocalRepositoryModel;
                var target = CreateTarget(
                    teamExplorerContext: CreateTeamExplorerContext(null));

                Assert.That(target.CurrentSession, Is.Null);
            }

            [Test]
            public void CurrentSessionChangesToNullIfNoPullRequestForCurrentBranch()
            {
                var service = CreatePullRequestService();
                var teamExplorerContext = CreateTeamExplorerContext(CreateRepositoryModel());
                var target = CreateTarget(
                    service: service,
                    teamExplorerContext: teamExplorerContext);
                Assert.That(target.CurrentSession, Is.Not.Null);

                (string owner, int number) newPullRequest = default;
                service.GetPullRequestForCurrentBranch(null).ReturnsForAnyArgs(Observable.Return(newPullRequest));
                teamExplorerContext.StatusChanged += Raise.Event();

                var session = target.CurrentSession;

                Assert.That(session, Is.Null);
            }

            [Test]
            public void CurrentSessionChangesToNullWhenRepoChangedToNull()
            {
                var teamExplorerContext = CreateTeamExplorerContext(CreateRepositoryModel());
                var target = CreateTarget(teamExplorerContext: teamExplorerContext);

                Assert.That(target.CurrentSession, Is.Not.Null);

                SetActiveRepository(teamExplorerContext, null);
                var session = target.CurrentSession;

                Assert.That(session, Is.Null);
            }

            [Test]
            public void CurrentSessionChangesWhenRepoChanged()
            {
                var teamExplorerContext = CreateTeamExplorerContext(CreateRepositoryModel());
                var target = CreateTarget(teamExplorerContext: teamExplorerContext);
                var session = target.CurrentSession;

                SetActiveRepository(teamExplorerContext, CreateRepositoryModel("https://github.com/owner/other"));

                Assert.That(session, Is.Not.SameAs(target.CurrentSession));
            }

            [Test]
            public void RepoChangedDoesntCreateNewSessionIfNotNecessary()
            {
                var teamExplorerContext = CreateTeamExplorerContext(CreateRepositoryModel());
                var target = CreateTarget(teamExplorerContext: teamExplorerContext);
                var session = target.CurrentSession;

                teamExplorerContext.StatusChanged += Raise.Event();

                Assert.That(session, Is.SameAs(target.CurrentSession));
            }

            [Test]
            public void RepoChangedHandlesNullRepository()
            {
                var teamExplorerContext = CreateTeamExplorerContext(CreateRepositoryModel());
                var target = CreateTarget(teamExplorerContext: teamExplorerContext);

                SetActiveRepository(teamExplorerContext, null);

                Assert.That(target.CurrentSession, Is.Null);
            }

            [Test]
            public void CreatesSessionWithCorrectRepositoryOwner()
            {
                var target = CreateTarget(service: CreatePullRequestService("this-owner"));

                Assert.That("this-owner", Is.EqualTo(target.CurrentSession.RepositoryOwner));
            }
        }

        public class TheGetLiveFileMethod : PullRequestSessionManagerTests
        {
            const string FilePath = "test.cs";

            [Test]
            public async Task BaseShaIsSet()
            {
                var textView = CreateTextView();
                var target = CreateTarget();
                var file = await target.GetLiveFile(FilePath, textView, textView.TextBuffer);

                Assert.That("BASESHA", Is.SameAs(file.BaseSha));
            }

            [Test]
            public async Task CommitShaIsSet()
            {
                var textView = CreateTextView();
                var target = CreateTarget();
                var file = await target.GetLiveFile(FilePath, textView, textView.TextBuffer);

                Assert.That("TIPSHA", Is.SameAs(file.CommitSha));
            }

            [Test]
            public async Task CommitShaIsNullIfModified()
            {
                var textView = CreateTextView();

                var target = CreateTarget(sessionService: CreateSessionService(isModified: true));
                var file = await target.GetLiveFile(FilePath, textView, textView.TextBuffer);

                Assert.That(file.CommitSha, Is.Null);
            }

            [Test]
            public async Task DiffIsSet()
            {
                var textView = CreateTextView();
                var contents = Encoding.UTF8.GetBytes("File contents");
                var diff = new List<DiffChunk>();
                var sessionService = CreateSessionService();

                sessionService.GetContents(textView.TextBuffer).Returns(contents);
                sessionService.GetPullRequestMergeBase(null, null).ReturnsForAnyArgs("MERGE_BASE");
                sessionService.Diff(
                    Arg.Any<LocalRepositoryModel>(),
                    "MERGE_BASE",
                    "HEADSHA",
                    FilePath,
                    contents).Returns(diff);

                var target = CreateTarget(sessionService: sessionService);
                var file = await target.GetLiveFile(FilePath, textView, textView.TextBuffer);

                Assert.That(diff, Is.SameAs(file.Diff));
            }

            [Test]
            public async Task InlineCommentThreadsIsSet()
            {
                var textView = CreateTextView();
                var sessionService = CreateSessionService();
                var threads = new List<IInlineCommentThreadModel>();
                var target = CreateTarget(sessionService: sessionService);

                sessionService.BuildCommentThreads(
                    target.CurrentSession.PullRequest,
                    FilePath,
                    Arg.Any<IReadOnlyList<DiffChunk>>(),
                    Arg.Any<string>())
                    .Returns(threads);

                var file = await target.GetLiveFile(FilePath, textView, textView.TextBuffer);

                Assert.That(threads, Is.SameAs(file.InlineCommentThreads));
            }

            [Test]
            public async Task CreatesTrackingPointsForThreads()
            {
                var textView = CreateTextView();
                var sessionService = CreateSessionService();
                var threads = new List<IInlineCommentThreadModel>
                    {
                        CreateInlineCommentThreadModel(1),
                        CreateInlineCommentThreadModel(2),
                    };

                var target = CreateTarget(sessionService: sessionService);

                sessionService.BuildCommentThreads(
                    target.CurrentSession.PullRequest,
                    FilePath,
                    Arg.Any<IReadOnlyList<DiffChunk>>(),
                    Arg.Any<string>())
                    .Returns(threads);

                var file = (PullRequestSessionLiveFile)await target.GetLiveFile(FilePath, textView, textView.TextBuffer);

                Assert.That(2, Is.EqualTo(file.TrackingPoints.Count));
            }

            [Test]
            public async Task MovingToNoRepositoryShouldNullOutProperties()
            {
                var textView = CreateTextView();
                var sessionService = CreateSessionService();
                var threads = new List<IInlineCommentThreadModel>();
                var teamExplorerContext = CreateTeamExplorerContext(CreateRepositoryModel());

                var target = CreateTarget(
                    sessionService: sessionService,
                    teamExplorerContext: teamExplorerContext);

                sessionService.BuildCommentThreads(
                    target.CurrentSession.PullRequest,
                    FilePath,
                    Arg.Any<IReadOnlyList<DiffChunk>>(),
                    Arg.Any<string>())
                    .Returns(threads);

                var file = (PullRequestSessionLiveFile)await target.GetLiveFile(FilePath, textView, textView.TextBuffer);

                Assert.That(file.BaseSha, Is.Not.Null);
                Assert.That(file.CommitSha, Is.Not.Null);
                Assert.That(file.Diff, Is.Not.Null);
                Assert.That(file.InlineCommentThreads, Is.Not.Null);
                Assert.That(file.TrackingPoints, Is.Not.Null);

                SetActiveRepository(teamExplorerContext, null);

                Assert.That(file.BaseSha, Is.Null);
                Assert.That(file.CommitSha, Is.Null);
                Assert.That(file.Diff, Is.Null);
                Assert.That(file.InlineCommentThreads, Is.Null);
                Assert.That(file.TrackingPoints, Is.Null);
            }

            [Test]
            public async Task ModifyingBufferMarksThreadsAsStaleAndSignalsRebuild()
            {
                var textView = CreateTextView();
                var sessionService = CreateSessionService();
                var rebuild = Substitute.For<ISubject<ITextSnapshot, ITextSnapshot>>();
                sessionService.CreateRebuildSignal().Returns(rebuild);

                var threads = new List<IInlineCommentThreadModel>
                    {
                        CreateInlineCommentThreadModel(1),
                        CreateInlineCommentThreadModel(2),
                    };

                var target = CreateTarget(sessionService: sessionService);

                sessionService.BuildCommentThreads(
                    target.CurrentSession.PullRequest,
                    FilePath,
                    Arg.Any<IReadOnlyList<DiffChunk>>(),
                    Arg.Any<string>())
                    .Returns(threads);

                var file = (PullRequestSessionLiveFile)await target.GetLiveFile(FilePath, textView, textView.TextBuffer);
                var linesChangedReceived = false;
                file.LinesChanged.Subscribe(x => linesChangedReceived = true);

                // Make the first tracking points return a different value so that the thread is marked as stale.
                var snapshot = textView.TextSnapshot;
                file.TrackingPoints[file.InlineCommentThreads[0]].GetPosition(snapshot).ReturnsForAnyArgs(5);

                SignalTextChanged(textView.TextBuffer);

                threads[0].Received().IsStale = true;
                threads[1].DidNotReceive().IsStale = true;

                Assert.That(linesChangedReceived, Is.True);
                file.Rebuild.Received().OnNext(Arg.Any<ITextSnapshot>());
            }

            [Test]
            public async Task RebuildSignalUpdatesCommitSha()
            {
                var textView = CreateTextView();
                var sessionService = CreateSessionService();
                sessionService.CreateRebuildSignal().Returns(new Subject<ITextSnapshot>());

                var threads = new List<IInlineCommentThreadModel>
                    {
                        CreateInlineCommentThreadModel(1),
                        CreateInlineCommentThreadModel(2),
                    };

                var target = CreateTarget(sessionService: sessionService);
                var file = (PullRequestSessionLiveFile)await target.GetLiveFile(FilePath, textView, textView.TextBuffer);

                Assert.That("TIPSHA", Is.SameAs(file.CommitSha));

                sessionService.IsUnmodifiedAndPushed(null, null, null).ReturnsForAnyArgs(false);
                file.Rebuild.OnNext(textView.TextBuffer.CurrentSnapshot);

                Assert.That(file.CommitSha, Is.Null);
            }

            [Test]
            public async Task ClosingTextViewDisposesFile()
            {
                var textView = CreateTextView();
                var target = CreateTarget();
                var file = (PullRequestSessionLiveFile)await target.GetLiveFile(FilePath, textView, textView.TextBuffer);

                var compositeDisposable = file.ToDispose as CompositeDisposable;
                Assert.That(compositeDisposable, Is.Not.Null);
                Assert.That(compositeDisposable.IsDisposed, Is.False);

                textView.Closed += Raise.Event();

                Assert.That(compositeDisposable.IsDisposed, Is.True);
            }

            [Test]
            public async Task InlineCommentThreadsAreLoadedFromCurrentSession()
            {
                var baseContents = @"Line 1
Line 2
Line 3
Line 4";
                var contents = @"Line 1
Line 2
Line 3 with comment
Line 4";
                var thread = CreateCommentThread(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment");

                using (var diffService = new FakeDiffService())
                {
                    var textView = CreateTextView(contents);
                    var pullRequest = CreatePullRequestModel(
                        CurrentBranchPullRequestNumber,
                        thread);

                    diffService.AddFile(FilePath, baseContents, "MERGE_BASE");

                    var target = CreateTarget(sessionService: CreateRealSessionService(diffService, pullRequest));
                    var file = (PullRequestSessionLiveFile)await target.GetLiveFile(FilePath, textView, textView.TextBuffer);

                    Assert.That(file.InlineCommentThreads.Count, Is.EqualTo(1));
                    Assert.That(file.InlineCommentThreads[0].LineNumber, Is.EqualTo(2));
                }
            }

            [Test, NUnit.Framework.Category("CodeCoverageFlake")]
            public async Task UpdatesInlineCommentThreadsFromEditorContent()
            {
                var baseContents = @"Line 1
Line 2
Line 3
Line 4";
                var contents = @"Line 1
Line 2
Line 3 with comment
Line 4";
                var editorContents = @"New Line 1
New Line 2
Line 1
Line 2
Line 3 with comment
Line 4";
                var comment = CreateCommentThread(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment");

                using (var diffService = new FakeDiffService())
                {
                    var textView = CreateTextView(contents);
                    var pullRequest = CreatePullRequestModel(
                        CurrentBranchPullRequestNumber,
                        comment);

                    diffService.AddFile(FilePath, baseContents, "MERGE_BASE");

                    var target = CreateTarget(sessionService: CreateRealSessionService(diffService, pullRequest));
                    var file = (PullRequestSessionLiveFile)await target.GetLiveFile(FilePath, textView, textView.TextBuffer);

                    Assert.That(1, Is.EqualTo(file.InlineCommentThreads.Count));
                    Assert.That(2, Is.EqualTo(file.InlineCommentThreads[0].LineNumber));

                    textView.TextSnapshot.GetText().Returns(editorContents);
                    SignalTextChanged(textView.TextBuffer);

                    var linesChanged = await file.LinesChanged.Take(1);

                    Assert.That(1, Is.EqualTo(file.InlineCommentThreads.Count));
                    Assert.That(4, Is.EqualTo(file.InlineCommentThreads[0].LineNumber));
                    Assert.That(
                        new[]
                        {
                                Tuple.Create(2, DiffSide.Right),
                                Tuple.Create(4, DiffSide.Right),
                        },
                        Is.EqualTo(linesChanged.ToArray()));
                }
            }

            [Test, NUnit.Framework.Category("CodeCoverageFlake")]
            public async Task UpdatesReviewCommentWithNewBody()
            {
                var baseContents = @"Line 1
Line 2
Line 3
Line 4";
                var contents = @"Line 1
Line 2
Line 3 with comment
Line 4";
                var comment = CreateCommentThread(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment", "Original Comment");
                var updatedComment = CreateCommentThread(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment", "Updated Comment");

                using (var diffService = new FakeDiffService())
                {
                    var textView = CreateTextView(contents);
                    var pullRequest = CreatePullRequestModel(
                        CurrentBranchPullRequestNumber,
                        comment);
                    var sessionService = CreateRealSessionService(diffService, pullRequest);

                    diffService.AddFile(FilePath, baseContents, "MERGE_BASE");

                    var target = CreateTarget(sessionService: sessionService);
                    var file = (PullRequestSessionLiveFile)await target.GetLiveFile(FilePath, textView, textView.TextBuffer);

                    Assert.That(file.InlineCommentThreads[0].Comments[0].Comment.Body, Is.EqualTo("Original Comment"));

                    pullRequest = CreatePullRequestModel(
                        CurrentBranchPullRequestNumber,
                        updatedComment);
                    sessionService.ReadPullRequestDetail(
                        Arg.Any<HostAddress>(),
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Any<int>()).Returns(pullRequest);
                    await target.CurrentSession.Refresh();

                    await file.LinesChanged.Take(1);

                    Assert.That("Updated Comment", Is.EqualTo(file.InlineCommentThreads[0].Comments[0].Comment.Body));
                }
            }

            [Test]
            public async Task AddsNewReviewCommentToThread()
            {
                var baseContents = @"Line 1
        Line 2
        Line 3
        Line 4";
                var contents = @"Line 1
        Line 2
        Line 3 with comment
        Line 4";
                var comment1 = CreateCommentThread(@"@@ -1,4 +1,4 @@
         Line 1
         Line 2
        -Line 3
        +Line 3 with comment", "Comment1");

                var comment2 = CreateCommentThread(@"@@ -1,4 +1,4 @@
         Line 1
         Line 2
        -Line 3
        +Line 3 with comment", "Comment2");

                using (var diffService = new FakeDiffService())
                {
                    var textView = CreateTextView(contents);
                    var pullRequest = CreatePullRequestModel(
                        CurrentBranchPullRequestNumber,
                        comment1);
                    var sessionService = CreateRealSessionService(diffService, pullRequest);

                    diffService.AddFile(FilePath, baseContents, "MERGE_BASE");

                    var target = CreateTarget(sessionService: sessionService);
                    var file = (PullRequestSessionLiveFile)await target.GetLiveFile(FilePath, textView, textView.TextBuffer);

                    Assert.That(1, Is.EqualTo(file.InlineCommentThreads[0].Comments.Count));

                    pullRequest = CreatePullRequestModel(
                        CurrentBranchPullRequestNumber,
                        comment1,
                        comment2);
                    sessionService.ReadPullRequestDetail(
                        Arg.Any<HostAddress>(),
                        Arg.Any<string>(),
                        Arg.Any<string>(),
                        Arg.Any<int>()).Returns(pullRequest);
                    await target.CurrentSession.Refresh();

                    var linesChanged = await file.LinesChanged.Take(1);

                    Assert.That(2, Is.EqualTo(file.InlineCommentThreads[0].Comments.Count));
                    Assert.That("Comment1", Is.EqualTo(file.InlineCommentThreads[0].Comments[0].Comment.Body));
                    Assert.That("Comment2", Is.EqualTo(file.InlineCommentThreads[0].Comments[1].Comment.Body));
                }
            }

            [Test]
            public async Task CommitShaIsUpdatedOnTextChange()
            {
                var textView = CreateTextView();
                var sessionService = CreateSessionService();

                var target = CreateTarget(sessionService: sessionService);
                var file = await target.GetLiveFile(FilePath, textView, textView.TextBuffer);

                Assert.That("TIPSHA", Is.EqualTo(file.CommitSha));

                sessionService.IsUnmodifiedAndPushed(null, null, null).ReturnsForAnyArgs(false);
                SignalTextChanged(textView.TextBuffer);

                Assert.That(file.CommitSha, Is.Null);
            }

            [Test]
            public async Task RefreshingCurrentSessionPullRequestTriggersLinesChanged()
            {
                var textView = CreateTextView();
                var sessionService = CreateSessionService();
                var expectedLineNumber = 2;
                var threads = new[]
                {
                            CreateInlineCommentThreadModel(expectedLineNumber),
                        };

                sessionService.BuildCommentThreads(null, null, null, null).ReturnsForAnyArgs(threads);

                var target = CreateTarget(sessionService: sessionService);
                var file = await target.GetLiveFile(FilePath, textView, textView.TextBuffer);
                var raised = false;
                var pullRequest = target.CurrentSession.PullRequest;

                file.LinesChanged.Subscribe(x => raised = x.Count == 1 && x[0].Item1 == expectedLineNumber);

                // LinesChanged should be raised even if the PullRequestDetailModel is the same.
                await target.CurrentSession.Refresh();

                Assert.That(raised, Is.True);
            }

            static PullRequestReviewThreadModel CreateCommentThread(
                string diffHunk,
                string body = "Comment",
                string filePath = FilePath)
            {
                var thread = new PullRequestReviewThreadModel
                {
                    DiffHunk = diffHunk,
                    Path = filePath,
                    OriginalCommitSha = "ORIG",
                    OriginalPosition = 1,
                };

                thread.Comments = new[]
                {
                    new PullRequestReviewCommentModel
                    {
                        Body = body,
                        Thread = thread,
                    },
                };

                return thread;
            }

            IPullRequestSessionService CreateRealSessionService(
                IDiffService diff,
                PullRequestDetailModel pullRequest)
            {
                var result = Substitute.ForPartsOf<PullRequestSessionService>(
                    Substitute.For<IGitService>(),
                    Substitute.For<IGitClient>(),
                    diff,
                    Substitute.For<IApiClientFactory>(),
                    Substitute.For<IGraphQLClientFactory>(),
                    Substitute.For<IUsageTracker>());
                result.CreateRebuildSignal().Returns(new Subject<ITextSnapshot>());
                result.GetPullRequestMergeBase(
                    Arg.Any<LocalRepositoryModel>(),
                    Arg.Any<PullRequestDetailModel>()).Returns("MERGE_BASE");
                result.ReadPullRequestDetail(
                    Arg.Any<HostAddress>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<int>()).Returns(pullRequest);
                result.ReadViewer(Arg.Any<HostAddress>()).Returns(new ActorModel());
                return result;
            }

            ITextView CreateTextView(string content = "Default content")
            {
                var snapshot = Substitute.For<ITextSnapshot>();
                snapshot.GetText().Returns(content);

                // We map snapshot positions to line numbers in tracking points, so return 100 as
                // the length so we can map lines 0-99.
                snapshot.Length.Returns(100);
                snapshot.GetLineFromLineNumber(0).ReturnsForAnyArgs(x =>
                {
                    var line = Substitute.For<ITextSnapshotLine>();
                    var lineNumber = x.Arg<int>();
                    var point = new SnapshotPoint(snapshot, lineNumber);
                    line.LineNumber.Returns(lineNumber);
                    line.Start.Returns(point);
                    return line;
                });
                snapshot.GetLineNumberFromPosition(0).ReturnsForAnyArgs(x => x.Arg<int>());
                snapshot.CreateTrackingPoint(0, 0).ReturnsForAnyArgs(x =>
                {
                    var point = Substitute.For<ITrackingPoint>();
                    point.GetPosition(snapshot).Returns(x.Arg<int>());
                    return point;
                });

                var textBuffer = Substitute.For<ITextBuffer>();
                textBuffer.Properties.Returns(new PropertyCollection());
                textBuffer.CurrentSnapshot.Returns(snapshot);

                var result = Substitute.For<ITextView>();
                result.TextBuffer.Returns(textBuffer);
                result.TextSnapshot.Returns(snapshot);

                return result;
            }

            IInlineCommentThreadModel CreateInlineCommentThreadModel(int lineNumber)
            {
                var result = Substitute.For<IInlineCommentThreadModel>();
                result.LineNumber.Returns(lineNumber);
                return result;
            }

            static void SignalTextChanged(ITextBuffer buffer)
            {
                var snapshot = buffer.CurrentSnapshot;
                var ev = new TextContentChangedEventArgs(snapshot, snapshot, EditOptions.None, null);
                buffer.Changed += Raise.EventWith(buffer, ev);
            }
        }

        public class TheGetSessionMethod : PullRequestSessionManagerTests
        {
            [Test]
            public async Task GetSessionReturnsSameSessionForSamePullRequest()
            {
                var target = CreateTarget();
                var newModel = CreatePullRequestModel(NotCurrentBranchPullRequestNumber);
                var result1 = await target.GetSession("owner", "repo", 5);
                var result2 = await target.GetSession("owner", "repo", 5);
                var result3 = await target.GetSession("owner", "repo", 6);

                Assert.That(result1, Is.SameAs(result2));
                Assert.That(result1, Is.Not.SameAs(result3));
            }

            [Test]
            public async Task GetSessionReturnsSameSessionForSamePullRequestOwnerCaseMismatch()
            {
                var target = CreateTarget();
                var newModel = CreatePullRequestModel(NotCurrentBranchPullRequestNumber);
                var result1 = await target.GetSession("owner", "repo", 5);
                var result2 = await target.GetSession("Owner", "repo", 5);
                var result3 = await target.GetSession("owner", "repo", 6);

                Assert.That(result1, Is.SameAs(result2));
                Assert.That(result1, Is.Not.SameAs(result3));
            }

            [Test]
            public async Task SessionCanBeCollected()
            {
                WeakReference<IPullRequestSession> weakSession = null;

                var target = CreateTarget();

                Func<Task> run = async () =>
                {
                    var newModel = CreatePullRequestModel(NotCurrentBranchPullRequestNumber);
                    var session = await target.GetSession("owner", "repo", 5);

                    Assert.That(session, Is.Not.Null);

                    weakSession = new WeakReference<IPullRequestSession>(session);
                };

                await run();
                GC.Collect();

                IPullRequestSession result;
                weakSession.TryGetTarget(out result);

                Assert.That(result, Is.Null);
            }

            [Test]
            public async Task GetSessionUpdatesCurrentSessionIfCurrentBranchIsPullRequestButWasNotMarked()
            {
                var service = CreatePullRequestService();
                var model = CreatePullRequestModel();
                var sessionService = CreateSessionService(model);

                service.GetPullRequestForCurrentBranch(null).ReturnsForAnyArgs(Observable.Empty<(string, int)>());

                var target = CreateTarget(service: service, sessionService: sessionService);

                Assert.That(target.CurrentSession, Is.Null);

                service.EnsureLocalBranchesAreMarkedAsPullRequests(Arg.Any<LocalRepositoryModel>(), model).Returns(Observable.Return(true));
                service.GetPullRequestForCurrentBranch(null).ReturnsForAnyArgs(Observable.Return(("owner", CurrentBranchPullRequestNumber)));

                var session = await target.GetSession("owner", "name", CurrentBranchPullRequestNumber);

                Assert.That(target.CurrentSession, Is.SameAs(session));
            }
        }

        PullRequestSessionManager CreateTarget(
            IPullRequestService service = null,
            IPullRequestSessionService sessionService = null,
            IConnectionManager connectionManager = null,
            ITeamExplorerContext teamExplorerContext = null)
        {
            service = service ?? CreatePullRequestService();
            sessionService = sessionService ?? CreateSessionService();
            connectionManager = connectionManager ?? CreateConnectionManager();
            teamExplorerContext = teamExplorerContext ?? CreateTeamExplorerContext(CreateRepositoryModel());

            return new PullRequestSessionManager(
                service,
                sessionService,
                teamExplorerContext);
        }

        PullRequestDetailModel CreatePullRequestModel(
            int number = 5,
            params PullRequestReviewThreadModel[] threads)
        {
            var result = new PullRequestDetailModel
            {
                Number = number,
                BaseRefName = "BASEREF",
                BaseRefSha = "BASESHA",
                HeadRefName = "HEADREF",
                HeadRefSha = "HEADSHA",
                Threads = threads,
            };

            if (threads.Length > 0)
            {
                result.Reviews = new[]
                {
                    new PullRequestReviewModel
                    {
                        Comments = threads.SelectMany(x => x.Comments).ToList(),
                        Author = CurrentUser,
                    },
                };
            }
            else
            {
                result.Reviews = Array.Empty<PullRequestReviewModel>();
            }

            return result;
        }

        IPullRequestService CreatePullRequestService(string owner = "owner")
        {
            var result = Substitute.For<IPullRequestService>();
            result.GetPullRequestForCurrentBranch(null).ReturnsForAnyArgs(Observable.Return((owner, CurrentBranchPullRequestNumber)));
            return result;
        }

        IConnectionManager CreateConnectionManager()
        {
            var connection = Substitute.For<IConnection>();
            connection.HostAddress.Returns(HostAddress.Create("https://github.com"));
            connection.IsLoggedIn.Returns(true);

            var result = Substitute.For<IConnectionManager>();
            result.GetConnection(connection.HostAddress).Returns(connection);
            return result;
        }

        IPullRequestSessionService CreateSessionService(
            PullRequestDetailModel pullRequest = null,
            bool isModified = false)
        {
            pullRequest = pullRequest ?? CreatePullRequestModel();

            var sessionService = Substitute.For<IPullRequestSessionService>();
            sessionService.CreateRebuildSignal().Returns(new Subject<ITextSnapshot>());
            sessionService.IsUnmodifiedAndPushed(null, null, null).ReturnsForAnyArgs(!isModified);
            sessionService.GetPullRequestMergeBase(null, null).ReturnsForAnyArgs("MERGE_BASE");
            sessionService.GetTipSha(null).ReturnsForAnyArgs("TIPSHA");
            sessionService.ReadPullRequestDetail(null, null, null, 0).ReturnsForAnyArgs(pullRequest);
            sessionService.ReadViewer(null).ReturnsForAnyArgs(CurrentUser);
            return sessionService;
        }

        LocalRepositoryModel CreateRepositoryModel(string cloneUrl = OwnerCloneUrl)
        {
            var cloneUrlString = new UriString(cloneUrl);
            return new LocalRepositoryModel
            {
                CloneUrl = cloneUrlString,
                Name = cloneUrlString.RepositoryName
            };
        }

        static ITeamExplorerContext CreateTeamExplorerContext(LocalRepositoryModel repo)
        {
            var teamExplorerContext = Substitute.For<ITeamExplorerContext>();
            teamExplorerContext.ActiveRepository.Returns(repo);
            return teamExplorerContext;
        }

        static void SetActiveRepository(ITeamExplorerContext teamExplorerContext, LocalRepositoryModel localRepositoryModel)
        {
            teamExplorerContext.ActiveRepository.Returns(localRepositoryModel);
            var eventArgs = new PropertyChangedEventArgs(nameof(teamExplorerContext.ActiveRepository));
            teamExplorerContext.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(teamExplorerContext, eventArgs);
        }
    }
}
