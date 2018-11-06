using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GitHub.InlineReviews.Tags;
using GitHub.Models;
using GitHub.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NSubstitute;
using NUnit.Framework;
using GitHub.InlineReviews.Margins;

namespace GitHub.InlineReviews.UnitTests.Tags
{
    public class InlineCommentTaggerTests
    {
        public class WithTextBufferInfo
        {
            [Test]
            public void FirstPassShouldReturnEmptyTags()
            {
                var file = CreateSessionFile();
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager(file, DiffSide.Right));

                var result = target.GetTags(CreateSpan(10));

                Assert.That(result, Is.Empty);
            }

            [Test]
            public void ShouldReturnShowInlineTagForRhs()
            {
                var file = CreateSessionFile();
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager(file, DiffSide.Right));

                // Line 10 has an existing RHS comment.
                var span = CreateSpan(10);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();

                Assert.That(result, Has.One.Items);
                Assert.That(result[0].Tag, Is.InstanceOf<ShowInlineCommentTag>());
            }

            [Test]
            public void ShouldReturnShowAnnotationTagForRhs()
            {
                var file = CreateSessionFile(withComments: false, withAnnotations:true);
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager(file, DiffSide.Right));

                // Line 10 has an existing Annotation comment.
                var span = CreateSpan(10);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();

                Assert.That(result, Has.One.Items);
                Assert.That(result[0].Tag, Is.InstanceOf<ShowInlineCommentTag>());
            }

            [Test]
            public void ShouldReturnAddNewCommentTagForAddedLineOnRhs()
            {
                var file = CreateSessionFile();
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager(file, DiffSide.Right));

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
                var file = CreateSessionFile();
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager(file, DiffSide.Right));

                // Line 13 has an delete diff entry.
                var span = CreateSpan(13);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();

                Assert.That(result, Is.Empty);
            }

            [Test]
            public void ShouldReturnShowInlineTagForLhs()
            {
                var file = CreateSessionFile();
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager(file, DiffSide.Left));

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
                var file = CreateSessionFile();
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager(file, DiffSide.Left));

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

            [Test]
            public void ShouldCallSessionGetFileWithCorrectCommitSha()
            {
                var sessionManager = CreateSessionManager(
                    CreateSessionFile(),
                    DiffSide.Right,
                    "123");
                var session = sessionManager.CurrentSession;
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    sessionManager);

                // Line 11 has an add diff entry.
                var span = CreateSpan(11);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();

                session.Received(1).GetFile("file.cs", "123");
            }

            [Test]
            public void ShouldAlwaysCallSessionGetFileWithHeadCommitShaForLeftHandSide()
            {
                var sessionManager = CreateSessionManager(
                    CreateSessionFile(),
                    DiffSide.Left,
                    "123");
                var session = sessionManager.CurrentSession;
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    sessionManager);

                // Line 11 has an add diff entry.
                var span = CreateSpan(11);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();

                session.Received(1).GetFile("file.cs", "HEAD");
            }

            static IPullRequestSessionManager CreateSessionManager(
                IPullRequestSessionFile file,
                DiffSide side,
                string bufferInfoCommitSha = "HEAD")
            {
                var session = Substitute.For<IPullRequestSession>();
                session.GetFile("file.cs", bufferInfoCommitSha).Returns(file);

                var info = new PullRequestTextBufferInfo(session, "file.cs", bufferInfoCommitSha, side);
                var result = Substitute.For<IPullRequestSessionManager>();
                result.CurrentSession.Returns(session);
                result.GetTextBufferInfo(null).ReturnsForAnyArgs(info);
                return result;
            }
        }

        public class WithoutTextBufferInfo
        {
            [Test]
            public void FirstPassShouldReturnEmptyTags()
            {
                var file = CreateSessionFile();
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager(file));

                var result = target.GetTags(CreateSpan(10));
                Assert.That(result, Is.Empty);
            }

            [Test]
            public void ShouldReturnShowInlineTagForComment()
            {
                var file = CreateSessionFile();
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager(file));

                // Line 10 has an existing RHS comment.
                var span = CreateSpan(10);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();

                Assert.That(result, Has.One.Items);

                var showInlineTag = result[0].Tag as ShowInlineCommentTag;
                Assert.That(showInlineTag, Is.Not.Null);
                Assert.That(showInlineTag.Thread, Is.Not.Null);
                Assert.That(showInlineTag.Annotations, Is.Null);
            }

            [Test]
            public void ShouldReturnShowInlineTagForAnnotation()
            {
                var file = CreateSessionFile(false, true);
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager(file));

                // Line 10 has an existing RHS annotation.
                var span = CreateSpan(10);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();

                Assert.That(result, Has.One.Items);

                var showInlineTag = result[0].Tag as ShowInlineCommentTag;
                Assert.That(showInlineTag, Is.Not.Null);
                Assert.That(showInlineTag.Thread, Is.Null);
                Assert.That(showInlineTag.Annotations, Is.Not.Null);
                Assert.That(showInlineTag.Annotations.Count, Is.EqualTo(1));
            }

            [Test]
            public void ShouldReturnShowInlineTagForTwoAnnotations()
            {
                var file = CreateSessionFile(false, true);
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager(file));

                // Line 20 has an existing RHS annotation.
                var span = CreateSpan(20);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();

                Assert.That(result, Has.One.Items);

                var showInlineTag = result[0].Tag as ShowInlineCommentTag;
                Assert.That(showInlineTag, Is.Not.Null);
                Assert.That(showInlineTag.Thread, Is.Null);
                Assert.That(showInlineTag.Annotations, Is.Not.Null);
                Assert.That(showInlineTag.Annotations.Count, Is.EqualTo(2));
            }

            [Test]
            public void ShouldReturnShowOneInlineTagForCommentAndAnnotation()
            {
                var file = CreateSessionFile(true, true);
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager(file));

                // Line 10 has an existing RHS comment.
                var span = CreateSpan(10);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();

                Assert.That(result, Has.One.Items);

                var showInlineTag = result[0].Tag as ShowInlineCommentTag;
                Assert.That(showInlineTag, Is.Not.Null);
                Assert.That(showInlineTag.Thread, Is.Not.Null);
                Assert.That(showInlineTag.Annotations, Is.Not.Null);
                Assert.That(showInlineTag.Annotations.Count, Is.EqualTo(1));
            }

            [Test]
            public void ShouldReturnAddNewCommentTagForAddedLine()
            {
                var file = CreateSessionFile();
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager(file));

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
                var file = CreateSessionFile();
                var target = new InlineCommentTagger(
                    Substitute.For<ITextView>(),
                    Substitute.For<ITextBuffer>(),
                    CreateSessionManager(file));

                // Line 13 has an delete diff entry.
                var span = CreateSpan(13);
                var firstPass = target.GetTags(span);
                var result = target.GetTags(span).ToList();
                Assert.That(result, Is.Empty);
            }

            [TestCase(true, true)]
            [TestCase(false, false)]
            public void ShouldRaiseTagsChangedOnFileLinesChanged(bool inlineCommentMarginVisible, bool expectRaised)
            {
                var file = CreateSessionFile();
                var manager = CreateSessionManager(file);
                var target = new InlineCommentTagger(
                    CreateTextView(inlineCommentMarginVisible),
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

                Assert.That(raised, Is.EqualTo(expectRaised));
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

            static ITextView CreateTextView(bool inlineCommentMarginVisible = true)
            {
                var textView = Substitute.For<ITextView>();
                textView.Options.GetOptionValue(InlineCommentTextViewOptions.MarginVisibleId).Returns(inlineCommentMarginVisible);
                return textView;
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
            result.Length.Returns(300);
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

        static IPullRequestSessionFile CreateSessionFile(bool withComments = true, bool withAnnotations = false)
        {
            var diffChunk = new DiffChunk
            {
                Lines =
                    {
                        // Line numbers here are 1-based. There is an add diff entry on lines 11 and 21
                        // and a delete entry on line 13.
                        new DiffLine { Type = DiffChangeType.Add, NewLineNumber = 11 + 1 },
                        new DiffLine { Type = DiffChangeType.Delete, OldLineNumber = 13 + 1 },
                        new DiffLine { Type = DiffChangeType.Add, NewLineNumber = 21 + 1 },
                    }
            };
            var diff = new List<DiffChunk> { diffChunk };

            var file = Substitute.For<IPullRequestSessionFile>();
            file.Diff.Returns(diff);

            if (withComments)
            {
                var rhsThread = Substitute.For<IInlineCommentThreadModel>();
                rhsThread.DiffLineType.Returns(DiffChangeType.Add);
                rhsThread.LineNumber.Returns(10);

                var lhsThread = Substitute.For<IInlineCommentThreadModel>();
                lhsThread.DiffLineType.Returns(DiffChangeType.Delete);
                lhsThread.LineNumber.Returns(12);

                // We have a comment to display on the right-hand-side of the diff view on line
                // 11 and a comment to display on line 13 on the left-hand-side.
                var threads = new List<IInlineCommentThreadModel> { rhsThread, lhsThread };

                file.InlineCommentThreads.Returns(threads);
            }

            if (withAnnotations)
            {
                var annotation1 = new InlineAnnotationModel(new CheckSuiteModel(), new CheckRunModel(), new CheckRunAnnotationModel(){EndLine = 11});

                var annotation2 = new InlineAnnotationModel(new CheckSuiteModel(), new CheckRunModel(), new CheckRunAnnotationModel() { EndLine = 21 });

                var annotation3 = new InlineAnnotationModel(new CheckSuiteModel(), new CheckRunModel(), new CheckRunAnnotationModel() { EndLine = 21 });

                var annotations = new List<InlineAnnotationModel> { annotation1, annotation2, annotation3 };

                file.InlineAnnotations.Returns(annotations);
            }

            file.LinesChanged.Returns(new Subject<IReadOnlyList<Tuple<int, DiffSide>>>());

            return file;
        }

    }
}
