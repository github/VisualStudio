using System;
using System.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Factories;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.UnitTests.TestDoubles;
using GitHub.Models;
using GitHub.Services;
using NSubstitute;
using NUnit.Framework;

namespace GitHub.InlineReviews.UnitTests.Services
{
    public class PullRequestSessionServiceTests
    {
        const int PullRequestNumber = 5;
        const string FilePath = "test.cs";

        public class TheBuildCommentThreadsMethod
        {
            [Test]
            public async Task MatchesReviewCommentOnOriginalLine()
            {
                var baseContents = @"Line 1
Line 2
Line 3
Line 4";
                var headContents = @"Line 1
Line 2
Line 3 with comment
Line 4";

                var comment = CreateCommentThread(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment");

                using (var diffService = new FakeDiffService(FilePath, baseContents))
                {
                    var diff = await diffService.Diff(FilePath, headContents);
                    var pullRequest = CreatePullRequest(FilePath, comment);
                    var target = CreateTarget(diffService);

                    var result = target.BuildCommentThreads(
                        pullRequest,
                        FilePath,
                        diff,
                        "HEAD_SHA");

                    var thread = result.Single();
                    Assert.That(thread.LineNumber, Is.EqualTo(2));
                }
            }

            [Test]
            public async Task IgnoreCommentsWithNoDiffLineContext()
            {
                var baseContents = "Line 1";
                var headContents = "Line 1";

                var comment = CreateCommentThread(@"@@ -10,7 +10,6 @@ class Program");

                using (var diffService = new FakeDiffService(FilePath, baseContents))
                {
                    var diff = await diffService.Diff(FilePath, headContents);
                    var pullRequest = CreatePullRequest(FilePath, comment);
                    var target = CreateTarget(diffService);

                    var result = target.BuildCommentThreads(
                        pullRequest,
                        FilePath,
                        diff,
                        "HEAD_SHA");

                    Assert.That(result, Is.Empty);
                }
            }

            [Test]
            public async Task MatchesReviewCommentOnDifferentLine()
            {
                var baseContents = @"Line 1
Line 2
Line 3
Line 4";
                var headContents = @"New Line 1
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

                using (var diffService = new FakeDiffService(FilePath, baseContents))
                {
                    var diff = await diffService.Diff(FilePath, headContents);
                    var pullRequest = CreatePullRequest(FilePath, comment);
                    var target = CreateTarget(diffService);

                    var result = target.BuildCommentThreads(
                        pullRequest,
                        FilePath,
                        diff,
                        "HEAD_SHA");

                    var thread = result.Single();
                    Assert.That(thread.LineNumber, Is.EqualTo(4));
                }
            }

            [Test]
            public async Task ReturnsLineNumberMinus1ForNonMatchingComment()
            {
                var baseContents = @"Line 1
Line 2
Line 3
Line 4";
                var headContents = @"Line 1
Line 2
Line 3 with comment
Line 4";

                var comment1 = CreateCommentThread(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment", position: 1);

                var comment2 = CreateCommentThread(@"@@ -1,4 +1,4 @@
-Line 1
 Line 2
-Line 3
+Line 3 with comment", position: 2);

                using (var diffService = new FakeDiffService(FilePath, baseContents))
                {
                    var diff = await diffService.Diff(FilePath, headContents);
                    var pullRequest = CreatePullRequest(FilePath, comment1, comment2);
                    var target = CreateTarget(diffService);

                    var result = target.BuildCommentThreads(
                        pullRequest,
                        FilePath,
                        diff,
                        "HEAD_SHA");

                    Assert.That(result.Count, Is.EqualTo(2));
                    Assert.That(result[1].LineNumber, Is.EqualTo(-1));
                }
            }

            [Test]
            public async Task HandlesDifferingPathSeparators()
            {
                var winFilePath = @"foo\test.cs";
                var gitHubFilePath = "foo/test.cs";

                var baseContents = @"Line 1
Line 2
Line 3
Line 4";
                var headContents = @"New Line 1
New Line 2
Line 1
Line 2
Line 3 with comment
Line 4";

                var comment = CreateCommentThread(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment", gitHubFilePath);

                using (var diffService = new FakeDiffService(winFilePath, baseContents))
                {
                    var diff = await diffService.Diff(winFilePath, headContents);
                    var pullRequest = CreatePullRequest(gitHubFilePath, comment);
                    var target = CreateTarget(diffService);

                    var result = target.BuildCommentThreads(
                        pullRequest,
                        winFilePath,
                        diff,
                        "HEAD_SHA");

                    var thread = result.First();
                    Assert.That(thread.LineNumber, Is.EqualTo(4));
                }
            }
        }

        public class TheUpdateCommentThreadsMethod
        {
            [Test]
            public async Task UpdatesWithNewLineNumber()
            {
                var baseContents = @"Line 1
Line 2
Line 3
Line 4";
                var headContents = @"Line 1
Line 2
Line 3 with comment
Line 4";
                var newHeadContents = @"Inserted Line
Line 1
Line 2
Line 3 with comment
Line 4";

                var comment = CreateCommentThread(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment");

                using (var diffService = new FakeDiffService(FilePath, baseContents))
                {
                    var diff = await diffService.Diff(FilePath, headContents);
                    var pullRequest = CreatePullRequest(FilePath, comment);
                    var target = CreateTarget(diffService);

                    var threads = target.BuildCommentThreads(
                        pullRequest,
                        FilePath,
                        diff,
                        "HEAD_SHA");

                    Assert.That(2, Is.EqualTo(threads[0].LineNumber));

                    diff = await diffService.Diff(FilePath, newHeadContents);
                    var changedLines = target.UpdateCommentThreads(threads, diff);

                    Assert.That(threads[0].LineNumber, Is.EqualTo(3));
                    Assert.That(changedLines.ToArray(), Is.EqualTo(new[]
                    {
                        Tuple.Create(2, DiffSide.Right),
                        Tuple.Create(3, DiffSide.Right)
                    }));
                }
            }

            [Test]
            public async Task UnmarksStaleThreads()
            {
                var baseContents = @"Line 1
Line 2
Line 3
Line 4";
                var headContents = @"Line 1
Line 2
Line 3 with comment
Line 4";

                var comment = CreateCommentThread(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment");

                using (var diffService = new FakeDiffService(FilePath, baseContents))
                {
                    var diff = await diffService.Diff(FilePath, headContents);
                    var pullRequest = CreatePullRequest(FilePath, comment);
                    var target = CreateTarget(diffService);

                    var threads = target.BuildCommentThreads(
                        pullRequest,
                        FilePath,
                        diff,
                        "HEAD_SHA");

                    threads[0].IsStale = true;
                    var changedLines = target.UpdateCommentThreads(threads, diff);

                    Assert.That(threads[0].IsStale, Is.False);
                    Assert.That(changedLines.ToArray(), Is.EqualTo(new[] { Tuple.Create(2, DiffSide.Right) }));
                }
            }
        }

        static PullRequestSessionService CreateTarget(IDiffService diffService)
        {
            return new PullRequestSessionService(
                Substitute.For<IGitService>(),
                Substitute.For<IGitClient>(),
                diffService,
                Substitute.For<IApiClientFactory>(),
                Substitute.For<IGraphQLClientFactory>(),
                Substitute.For<IUsageTracker>());
        }

        static PullRequestReviewThreadModel CreateCommentThread(
            string diffHunk,
            string filePath = FilePath,
            string body = "Comment",
            int position = 1)
        {
            return new PullRequestReviewThreadModel
            {
                DiffHunk = diffHunk,
                Path = filePath,
                OriginalCommitSha = "ORIG",
                OriginalPosition = position,
                Comments = new[]
                {
                    new PullRequestReviewCommentModel
                    {
                        Body = body,
                        Author = new ActorModel { Login = "Author" },
                    }
                },
            };
        }

        static PullRequestDetailModel CreatePullRequest(
            string filePath,
            params PullRequestReviewThreadModel[] threads)
        {
            return new PullRequestDetailModel
            {
                Number = PullRequestNumber,
                BaseRefName = "BASE",
                BaseRefSha = "BASE_SHA",
                BaseRepositoryOwner = "owner",
                HeadRefName = "HEAD",
                HeadRefSha = "HEAD_SHA",
                HeadRepositoryOwner = "owner",
                ChangedFiles = new []
                {
                    new PullRequestFileModel { FileName = filePath },
                    new PullRequestFileModel { FileName = "other.cs" },
                },
                Threads = threads,
                Reviews = new[]
                {
                    new PullRequestReviewModel
                    {
                        Comments = threads.SelectMany(x => x.Comments).ToList(),
                    },
                },
            };
        }
    }
}
