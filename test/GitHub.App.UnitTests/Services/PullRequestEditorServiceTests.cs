using System.Collections.Generic;
using GitHub.Services;
using NUnit.Framework;
using NSubstitute;
using Microsoft.VisualStudio.Editor;
using GitHub.Commands;
using Microsoft.VisualStudio.Text.Editor;

public class PullRequestEditorServiceTests
{
    public class TheFindNearestMatchingLineMethod
    {
        [TestCase(new[] { "line" }, new[] { "line" }, 0, 0, 1, Description = "Match same line")]
        [TestCase(new[] { "line" }, new[] { "line_no_match" }, 0, -1, 0, Description = "No matching line")]
        [TestCase(new[] { "line" }, new[] { "", "line" }, 0, 1, 1, Description = "Match line moved up")]
        [TestCase(new[] { "", "line" }, new[] { "line" }, 1, 0, 1, Description = "Match line moved down")]
        [TestCase(new[] { "line", "line" }, new[] { "line", "line" }, 0, 0, 2, Description = "Match nearest line")]
        [TestCase(new[] { "line", "line" }, new[] { "line", "line" }, 1, 1, 2, Description = "Match nearest line")]
        [TestCase(new[] { "line" }, new[] { "line" }, 1, 0, 1, Description = "Treat after last line the same as last line")]
        public void FindNearestMatchingLine(IList<string> fromLines, IList<string> toLines, int line,
            int expectNearestLine, int expectMatchingLines)
        {
            var target = CreateNavigationService();

            int matchedLines;
            var nearestLine = target.FindNearestMatchingLine(fromLines, toLines, line, out matchedLines);

            Assert.That(nearestLine, Is.EqualTo(expectNearestLine));
            Assert.That(matchedLines, Is.EqualTo(expectMatchingLines));
        }
    }

    public class TheFindMatchingLineMethod
    {
        [TestCase(new[] { "void method()", "code" }, new[] { "void method()", "// code" }, 1, 1)]
        [TestCase(new[] { "void method()", "code" }, new[] { "void method()" }, 1, 0, Description = "Keep within bounds")]
        [TestCase(new[] { "code" }, new[] { "// code" }, 0, -1)]
        [TestCase(new[] { "line", "line" }, new[] { "line", "line" }, 0, 0, Description = "Match nearest line")]
        [TestCase(new[] { "line", "line" }, new[] { "line", "line" }, 1, 1, Description = "Match nearest line")]
        public void FindNearestMatchingLine(IList<string> fromLines, IList<string> toLines, int line,
            int matchingLine)
        {
            var target = CreateNavigationService();

            var nearestLine = target.FindMatchingLine(fromLines, toLines, line, matchLinesAbove: 1);

            Assert.That(nearestLine, Is.EqualTo(matchingLine));
        }
    }

    static PullRequestEditorService CreateNavigationService()
    {
        var sp = Substitute.For<IGitHubServiceProvider>();
        var pullRequestService = Substitute.For<IPullRequestService>();
        var vsEditorAdaptersFactory = Substitute.For<IVsEditorAdaptersFactoryService>();
        var statusBar = Substitute.For<IStatusBarNotificationService>();
        var openFileInSolutionCommand = Substitute.For<IGoToSolutionOrPullRequestFileCommand>();
        var editorOptionsFactoryService = Substitute.For<IEditorOptionsFactoryService>();
        var draftStore = Substitute.For<IMessageDraftStore>();
        var peekService = Substitute.For<IInlineCommentPeekService>();
        var usageTracker = Substitute.For<IUsageTracker>();
        return new PullRequestEditorService(
            sp,
            pullRequestService,
            vsEditorAdaptersFactory,
            statusBar,
            openFileInSolutionCommand,
            editorOptionsFactoryService,
            draftStore,
            peekService,
            usageTracker);
    }
}
