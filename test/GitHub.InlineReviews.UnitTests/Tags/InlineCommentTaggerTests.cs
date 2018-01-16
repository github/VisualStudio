using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.Tags;
using GitHub.Models;
using GitHub.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NSubstitute;
using NUnit.Framework;

namespace GitHub.InlineReviews.UnitTests.Tags
{
    public class InlineCommentTaggerTests
    {
        public class WithTextBufferInfo
        {
            [Test]
            public void FirstPassShouldReturnEmptyTags()
            {
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager(DiffSide.Right));

                var result = target.GetTags(CreateSpan(10));

                Assert.That(result, Is.Empty);
            }

            [Test]
            public void ShouldReturnShowCommentTagForRhs()
            {
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager(DiffSide.Right));

                // Line 10 has an existing RHS comment.
                var span = CreateSpan(10);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();

                Assert.That(result, Has.One.Items);
                Assert.That(result[0].Tag, Is.InstanceOf<ShowInlineCommentTag>());
            }

            [Test]
            public void ShouldReturnAddNewCommentTagForAddedLineOnRhs()
            {
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager(DiffSide.Right));

                // Line 11 has an add diff entry.
                var span = CreateSpan(11);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();

                Assert.That(result, Has.One.Items);
                Assert.That(result[0].Tag, Is.InstanceOf<AddInlineCommentTag>());
            }

            [Test]
            public void ShouldNotReturnAddNewCommentTagForDeletedLineOnRhs()
            {
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager(DiffSide.Right));

                // Line 13 has an delete diff entry.
                var span = CreateSpan(13);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();

                Assert.That(result, Is.Empty);
            }

            [Test]
            public void ShouldReturnShowCommentTagForLhs()
            {
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager(DiffSide.Left));

                // Line 12 has an existing LHS comment.
                var span = CreateSpan(12);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();

                Assert.That(result, Has.One.Items);
                Assert.That(result[0].Tag, Is.InstanceOf<ShowInlineCommentTag>());
            }

            [Test]
            public void ShouldReturnAddCommentTagForLhs()
            {
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager(DiffSide.Left));

                // Line 13 has an delete diff entry.
                var span = CreateSpan(13);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();

                Assert.That(result, Has.One.Items);
                Assert.That(result[0].Tag, Is.InstanceOf<AddInlineCommentTag>());
            }

            [Test]
            public void ShouldRaiseTagsChangedOnFileLinesChanged()
            {
                var file = CreateSessionFile();
                var manager = CreateSessionManager(file, DiffSide.Right);
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    CreateBuffer(),
                    manager);

                var session = manager.GetTextBufferInfo(null).Session;
                var span = CreateSpan(14);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();
                var raised = false;

                target.TagsChanged += (s, e) => raised = e.Span.Start == 140;
                ((ISubject<IReadOnlyList<Tuple<int, DiffSide>>>)file.LinesChanged).OnNext(new[]
                {
                    Tuple.Create(14, DiffSide.Right),
                });

                Assert.True(raised);
            }

            static IPullRequestSessionFile CreateSessionFile()
            {
                var diffChunk = new DiffChunk
                {
                    Lines =
                    {
                        // Line numbers here are 1-based. There is an add diff entry on line 11
                        // and a delete entry on line 13.
                        new DiffLine { Type = DiffChangeType.Add, NewLineNumber = 11 + 1 },
                        new DiffLine { Type = DiffChangeType.Delete, OldLineNumber = 13 + 1 },
                    }
                };
                var diff = new List<DiffChunk> { diffChunk };

                var rhsThread = Substitute.For<IInlineCommentThreadModel>();
                rhsThread.DiffLineType.Returns(DiffChangeType.Add);
                rhsThread.LineNumber.Returns(10);

                var lhsThread = Substitute.For<IInlineCommentThreadModel>();
                lhsThread.DiffLineType.Returns(DiffChangeType.Delete);
                lhsThread.LineNumber.Returns(12);

                // We have a comment to display on the right-hand-side of the diff view on line
                // 11 and a comment to display on line 13 on the left-hand-side.
                var threads = new List<IInlineCommentThreadModel> { rhsThread, lhsThread };

                var file = Substitute.For<IPullRequestSessionFile>();
                file.Diff.Returns(diff);
                file.InlineCommentThreads.Returns(threads);
                file.LinesChanged.Returns(new Subject<IReadOnlyList<Tuple<int, DiffSide>>>());

                return file;
            }

            static IPullRequestSessionManager CreateSessionManager(DiffSide side)
            {
                var file = CreateSessionFile();
                return CreateSessionManager(file, side);
            }

            static IPullRequestSessionManager CreateSessionManager(
                IPullRequestSessionFile file,
                DiffSide side)
            {
                var session = Substitute.For<IPullRequestSession>();
                session.GetFile("file.cs").Returns(file);

                var info = new PullRequestTextBufferInfo(session, "file.cs", side);
                var result = Substitute.For<IPullRequestSessionManager>();
                result.GetTextBufferInfo(null).ReturnsForAnyArgs(info);
                return result;
            }
        }

        public class WithoutTextBufferInfo
        {
            [Test]
            public void FirstPassShouldReturnEmptyTags()
            {
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager());

                var result = target.GetTags(CreateSpan(10));
                Assert.That(result, Is.Empty);
            }

            [Test]
            public void ShouldReturnShowCommentTag()
            {
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager());

                // Line 10 has an existing RHS comment.
                var span = CreateSpan(10);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();

                Assert.That(result, Has.One.Items);
                Assert.That(result[0].Tag, Is.InstanceOf<ShowInlineCommentTag>());
            }

            [Test]
            public void ShouldReturnAddNewCommentTagForAddedLine()
            {
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager());

                // Line 11 has an add diff entry.
                var span = CreateSpan(11);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();

                Assert.That(result, Has.One.Items);
                Assert.That(result[0].Tag, Is.InstanceOf<AddInlineCommentTag>());
            }

            [Test]
            public void ShouldNotReturnAddNewCommentTagForDeletedLineOnRhs()
            {
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager());

                // Line 13 has an delete diff entry.
                var span = CreateSpan(13);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();
                 Assert.That(result, Is.Empty);
            }

            [Test]
            public void ShouldRaiseTagsChangedOnFileLinesChanged()
            {
                var file = CreateSessionFile();
                var manager = CreateSessionManager(file);
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    CreateBuffer(),
                    manager);

                var span = CreateSpan(14);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();
                var raised = false;

                target.TagsChanged += (s, e) => raised = e.Span.Start == 140;
                ((ISubject<IReadOnlyList<Tuple<int, DiffSide>>>)file.LinesChanged).OnNext(new[]
                {
                    Tuple.Create(14, DiffSide.Right),
                });

                Assert.True(raised);
            }

            [Test]
            public void ShouldNotRaiseTagsChangedOnLeftHandSideLinesChanged()
            {
                var file = CreateSessionFile();
                var manager = CreateSessionManager(file);
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    CreateBuffer(),
                    manager);

                var span = CreateSpan(14);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();
                var raised = false;

                target.TagsChanged += (s, e) => raised = true;
                ((ISubject<IReadOnlyList<Tuple<int, DiffSide>>>)file.LinesChanged).OnNext(new[]
                {
                    Tuple.Create(14, DiffSide.Left),
                });

                Assert.False(raised);
            }

            static IPullRequestSessionFile CreateSessionFile()
            {
                var diffChunk = new DiffChunk
                {
                    Lines =
                    {
                        // Line numbers here are 1-based. There is an add diff entry on line 11
                        // and a delete entry on line 13.
                        new DiffLine { Type = DiffChangeType.Add, NewLineNumber = 11 + 1 },
                        new DiffLine { Type = DiffChangeType.Delete, OldLineNumber = 13 + 1 },
                    }
                };
                var diff = new List<DiffChunk> { diffChunk };

                var rhsThread = Substitute.For<IInlineCommentThreadModel>();
                rhsThread.DiffLineType.Returns(DiffChangeType.Add);
                rhsThread.LineNumber.Returns(10);

                var lhsThread = Substitute.For<IInlineCommentThreadModel>();
                lhsThread.DiffLineType.Returns(DiffChangeType.Delete);
                lhsThread.LineNumber.Returns(12);

                // We have a comment to display on the right-hand-side of the diff view on line
                // 11 and a comment to display on line 13 on the left-hand-side.
                var threads = new List<IInlineCommentThreadModel> { rhsThread, lhsThread };

                var file = Substitute.For<IPullRequestSessionFile>();
                file.Diff.Returns(diff);
                file.InlineCommentThreads.Returns(threads);
                file.LinesChanged.Returns(new Subject<IReadOnlyList<Tuple<int, DiffSide>>>());

                return file;
            }

            static IPullRequestSessionManager CreateSessionManager()
            {
                var file = CreateSessionFile();
                return CreateSessionManager(file);
            }

            static IPullRequestSessionManager CreateSessionManager(IPullRequestSessionFile file)
            {
                var result = Substitute.For<IPullRequestSessionManager>();
                result.GetLiveFile("file.cs", Arg.Any<ITextView>(), Arg.Any<ITextBuffer>())
                    .Returns(Task.FromResult(file));
                result.GetRelativePath(null).ReturnsForAnyArgs("file.cs");
                return result;
            }
        }

        static ITextSnapshot CreateSnapshot()
        {
            // We pretend that each line has 10 chars and there are 20 lines.
            var result = Substitute.For<ITextSnapshot>();
            result.Length.Returns(200);
            result.GetLineFromPosition(0).ReturnsForAnyArgs(x => CreateLine(result, x.Arg<int>() / 10));
            result.GetLineFromLineNumber(0).ReturnsForAnyArgs(x => CreateLine(result, x.Arg<int>()));
            return result;
        }

        static NormalizedSnapshotSpanCollection CreateSpan(int lineNumber)
        {
            var snapshot = CreateSnapshot();
            var span = new Span(lineNumber * 10, 9);
            return new NormalizedSnapshotSpanCollection(snapshot, span);
        }

        static ITextBuffer CreateBuffer()
        {
            var snapshot = CreateSnapshot();
            var result = Substitute.For<ITextBuffer>();
            result.CurrentSnapshot.Returns(snapshot);
            return result;
        }

        static ITextSnapshotLine CreateLine(ITextSnapshot snapshot, int lineNumber)
        {
            var result = Substitute.For<ITextSnapshotLine>();
            var start = new SnapshotPoint(snapshot, lineNumber * 10);
            var end = new SnapshotPoint(snapshot, (lineNumber * 10) + 9);
            result.LineNumber.Returns(lineNumber);
            result.Start.Returns(start);
            result.End.Returns(end);
            return result;
        }
    }
}
