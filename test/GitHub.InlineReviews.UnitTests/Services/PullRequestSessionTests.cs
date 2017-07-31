using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.UnitTests.TestDoubles;
using GitHub.Models;
using LibGit2Sharp;
using NSubstitute;
using Xunit;

namespace GitHub.InlineReviews.UnitTests.Services
{
    public class PullRequestSessionTests
    {
        const int PullRequestNumber = 5;
        const string RepoUrl = "https://foo.bar/owner/repo";
        const string FilePath = "test.cs";

        public class TheGetFileMethod
        {
            [Fact]
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

                var comment = CreateComment(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment");

                using (var diffService = new FakeDiffService())
                {
                    var pullRequest = CreatePullRequest(comment);
                    var service = CreateService(diffService);

                    diffService.AddFile(FilePath, baseContents);

                    var target = new PullRequestSession(
                        service,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
                        "owner",
                        true);

                    var editor = new FakeEditorContentSource(headContents);
                    var file = await target.GetFile(FilePath, editor);
                    var thread = file.InlineCommentThreads.First();
                    Assert.Equal(2, thread.LineNumber);
                }
            }

            [Fact]
            public async Task MatchesReviewCommentOnOriginalLineGettingContentFromDisk()
            {
                var baseContents = @"Line 1
Line 2
Line 3
Line 4";
                var headContents = @"Line 1
Line 2
Line 3 with comment
Line 4";

                var comment = CreateComment(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment");

                using (var diffService = new FakeDiffService())
                {
                    var pullRequest = CreatePullRequest(comment);
                    var service = CreateService(diffService);

                    diffService.AddFile(FilePath, baseContents);
                    service.ReadFileAsync(FilePath).Returns(Encoding.UTF8.GetBytes(headContents));

                    var target = new PullRequestSession(
                        service,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
                        "owner",
                        true);

                    var file = await target.GetFile(FilePath);
                    var thread = file.InlineCommentThreads.First();

                    Assert.Equal(2, thread.LineNumber);
                }
            }

            [Fact]
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

                var comment = CreateComment(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment");


                using (var diffService = new FakeDiffService())
                {
                    var pullRequest = CreatePullRequest(comment);
                    var service = CreateService(diffService);

                    diffService.AddFile(FilePath, baseContents);

                    var target = new PullRequestSession(
                        service,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
                        "owner",
                        true);

                    var editor = new FakeEditorContentSource(headContents);
                    var file = await target.GetFile(FilePath, editor);
                    var thread = file.InlineCommentThreads.First();

                    Assert.Equal(4, thread.LineNumber);
                }
            }

            [Fact]
            public async Task UpdatesReviewCommentWithEditorContents()
            {
                var baseContents = @"Line 1
Line 2
Line 3
Line 4";
                var diskContents = @"Line 1
Line 2
Line 3 with comment
Line 4";
                var editorContents = @"New Line 1
New Line 2
Line 1
Line 2
Line 3 with comment
Line 4";

                var comment = CreateComment(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment");

                using (var diffService = new FakeDiffService())
                {
                    var pullRequest = CreatePullRequest(comment);
                    var service = CreateService(diffService);

                    diffService.AddFile(FilePath, baseContents);

                    var target = new PullRequestSession(
                        service,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
                        "owner",
                        true);

                    var editor = new FakeEditorContentSource(diskContents);
                    var file = await target.GetFile(FilePath, editor);
                    var thread = file.InlineCommentThreads.First();

                    Assert.Equal(2, thread.LineNumber);
                    editor.SetContent(editorContents);

                    await target.UpdateEditorContent(FilePath);

                    Assert.Equal(4, thread.LineNumber);
                }
            }

            [Fact]
            public async Task UpdatesReviewCommentWithContentsFromGitWhenBranchNotCheckedOut()
            {
                var baseContents = @"Line 1
Line 2
Line 3
Line 4";
                var gitContents = Encoding.UTF8.GetBytes(@"Line 1
Line 2
Line 3 with comment
Line 4");
                var editorContents = @"Editor content";

                var comment = CreateComment(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment");

                using (var diffService = new FakeDiffService())
                {
                    var pullRequest = CreatePullRequest(comment);
                    var service = CreateService(diffService);

                    diffService.AddFile(FilePath, baseContents);

                    // Because the PR branch isn't checked out, the file contents should be read
                    // from git and not the editor or disk.
                    service.ExtractFileFromGit(Arg.Any<ILocalRepositoryModel>(), PullRequestNumber, "HEAD_SHA", FilePath)
                        .Returns(Task.FromResult(gitContents));

                    var target = new PullRequestSession(
                        service,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
                        "owner",
                        isCheckedOut: false);

                    var editor = new FakeEditorContentSource(editorContents);
                    var file = await target.GetFile(FilePath, editor);
                    var thread = file.InlineCommentThreads.First();

                    Assert.Equal(2, thread.LineNumber);
                }
            }

            [Fact]
            public async Task UpdatesReviewCommentWithNewBody()
            {
                var baseContents = @"Line 1
Line 2
Line 3
Line 4";
                var editorContents = @"Line 1
Line 2
Line 3 with comment
Line 4";

                var originalComment = CreateComment(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment", "Original Comment");

                var updatedComment = CreateComment(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment", "Updated Comment");


                using (var diffService = new FakeDiffService())
                {
                    var pullRequest = CreatePullRequest(originalComment);
                    var service = CreateService(diffService);

                    diffService.AddFile(FilePath, baseContents);

                    var target = new PullRequestSession(
                        service,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
                        "owner",
                        true);

                    var editor = new FakeEditorContentSource(editorContents);
                    var file = await target.GetFile(FilePath, editor);

                    var thread = file.InlineCommentThreads.Single();

                    Assert.Equal("Original Comment", thread.Comments.Single().Body);
                    Assert.Equal(2, thread.LineNumber);

                    pullRequest = CreatePullRequest(updatedComment);
                    await target.Update(pullRequest);
                    thread = file.InlineCommentThreads.Single();

                    Assert.Equal("Updated Comment", thread.Comments.Single().Body);
                    Assert.Equal(2, thread.LineNumber);
                }
            }

            [Fact]
            public async Task AddsNewReviewCommentToThread()
            {
                var baseContents = @"Line 1
Line 2
Line 3
Line 4";
                var editorContents = @"Line 1
Line 2
Line 3 with comment
Line 4";

                var comment1 = CreateComment(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment", "Comment1");

                var comment2 = CreateComment(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment", "Comment2");


                using (var diffService = new FakeDiffService())
                {
                    var pullRequest = CreatePullRequest(comment1);
                    var service = CreateService(diffService);

                    diffService.AddFile(FilePath, baseContents);

                    var target = new PullRequestSession(
                        service,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
                        "owner",
                        true);

                    var editor = new FakeEditorContentSource(editorContents);
                    var file = await target.GetFile(FilePath, editor);

                    var thread = file.InlineCommentThreads.Single();

                    Assert.Equal("Comment1", thread.Comments.Single().Body);
                    Assert.Equal(2, thread.LineNumber);

                    pullRequest = CreatePullRequest(comment1, comment2);
                    await target.Update(pullRequest);
                    thread = file.InlineCommentThreads.Single();

                    Assert.Equal(2, thread.Comments.Count);
                    Assert.Equal(new[] { "Comment1", "Comment2" }, thread.Comments.Select(x => x.Body).ToArray());
                    Assert.Equal(2, thread.LineNumber);
                }
            }

            [Fact]
            public async Task CommitShaIsSetIfUnmodified()
            {
                var baseContents = @"Line 1
Line 2
Line 3
Line 4";
                var headContents = @"Line 1
Line 2
Line 3 with comment
Line 4";

                using (var diffService = new FakeDiffService())
                {
                    var pullRequest = CreatePullRequest();
                    var service = CreateService(diffService);

                    diffService.AddFile(FilePath, baseContents);
                    service.IsUnmodifiedAndPushed(Arg.Any<ILocalRepositoryModel>(), FilePath, Arg.Any<byte[]>()).Returns(true);

                    var target = new PullRequestSession(
                        service,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
                        "owner",
                        true);

                    var editor = new FakeEditorContentSource(headContents);
                    var file = await target.GetFile(FilePath, editor);
                    Assert.Equal("BRANCH_TIP", file.CommitSha);
                }
            }

            [Fact]
            public async Task CommitShaIsNullIfModified()
            {
                var baseContents = @"Line 1
Line 2
Line 3
Line 4";
                var headContents = @"Line 1
Line 2
Line 3 with comment
Line 4";


                using (var diffService = new FakeDiffService())
                {
                    var pullRequest = CreatePullRequest();
                    var service = CreateService(diffService);

                    diffService.AddFile(FilePath, baseContents);
                    service.IsUnmodifiedAndPushed(Arg.Any<ILocalRepositoryModel>(), FilePath, Arg.Any<byte[]>()).Returns(false);

                    var target = new PullRequestSession(
                        service,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
                        "owner",
                        true);

                    var editor = new FakeEditorContentSource(headContents);
                    var file = await target.GetFile(FilePath, editor);
                    Assert.Null(file.CommitSha);
                }
            }

            [Fact]
            public async Task CommitShaIsNullWhenChangedToModified()
            {
                var baseContents = @"Line 1
Line 2
Line 3
Line 4";
                var headContents = Encoding.UTF8.GetBytes(@"Line 1
Line 2
Line 3 with comment
Line 4");
                var editorContents = Encoding.UTF8.GetBytes(@"Line 1
Line 2
Line 3 with comment
Line 4 with comment");

                using (var diffService = new FakeDiffService())
                {
                    var pullRequest = CreatePullRequest();
                    var service = CreateService(diffService);

                    diffService.AddFile(FilePath, baseContents);
                    service.IsUnmodifiedAndPushed(Arg.Any<ILocalRepositoryModel>(), FilePath, headContents).Returns(true);
                    service.IsUnmodifiedAndPushed(Arg.Any<ILocalRepositoryModel>(), FilePath, editorContents).Returns(false);

                    var target = new PullRequestSession(
                        service,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
                        "owner",
                        true);

                    var editor = new FakeEditorContentSource(headContents);
                    var file = await target.GetFile(FilePath, editor);

                    Assert.Equal("BRANCH_TIP", file.CommitSha);

                    editor.SetContent(editorContents);
                    await target.UpdateEditorContent(FilePath);

                    Assert.Null(file.CommitSha);
                }
            }

            [Fact]
            public async Task CommitShaIsReadFromPullRequestModelIfBranchNotCheckedOut()
            {
                var baseContents = @"Line 1
Line 2
Line 3
Line 4";
                var headContents = @"Line 1
Line 2
Line 3 with comment
Line 4";

                using (var diffService = new FakeDiffService())
                {
                    var pullRequest = CreatePullRequest();
                    var service = CreateService(diffService);

                    diffService.AddFile(FilePath, baseContents);
                    service.IsUnmodifiedAndPushed(Arg.Any<ILocalRepositoryModel>(), FilePath, Arg.Any<byte[]>()).Returns(false);

                    var target = new PullRequestSession(
                        service,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
                        "owner",
                        isCheckedOut: false);

                    var editor = new FakeEditorContentSource(headContents);
                    var file = await target.GetFile(FilePath, editor);
                    Assert.Equal("HEAD_SHA", file.CommitSha);
                }
            }

        }

        public class TheAddCommentMethod
        {
            [Fact]
            public async Task UpdatesFileWithNewThread()
            {
                var comment = CreateComment(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment", "New Comment");

                using (var diffService = new FakeDiffService())
                {
                    var target = await CreateTarget(diffService);
                    var file = await target.GetFile(FilePath);

                    Assert.Empty(file.InlineCommentThreads);

                    await target.AddComment(comment);

                    Assert.Equal(1, file.InlineCommentThreads.Count);
                    Assert.Equal(2, file.InlineCommentThreads[0].LineNumber);
                    Assert.Equal(1, file.InlineCommentThreads[0].Comments.Count);
                    Assert.Equal("New Comment", file.InlineCommentThreads[0].Comments[0].Body);
                }
            }

            async Task<PullRequestSession> CreateTarget(FakeDiffService diffService)
            {
                var baseContents = @"Line 1
Line 2
Line 3
Line 4";
                var headContents = @"Line 1
Line 2
Line 3 with comment
Line 4";

                var pullRequest = CreatePullRequest();
                var service = CreateService(diffService);

                diffService.AddFile(FilePath, baseContents);

                var target = new PullRequestSession(
                    service,
                    Substitute.For<IAccount>(),
                    pullRequest,
                    Substitute.For<ILocalRepositoryModel>(),
                    "owner",
                    true);

                var editor = new FakeEditorContentSource(headContents);
                var file = await target.GetFile(FilePath, editor);
                return target;
            }
        }

        public class TheUpdateMethod
        {
            [Fact]
            public async Task DoesntThrowIfGetFileCalledDuringUpdate()
            {
                var comment = CreateComment(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment");

                using (var diffService = new FakeDiffService())
                {
                    var pullRequest = CreatePullRequest(comment);
                    var service = CreateService(diffService);

                    var target = new PullRequestSession(
                        service,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
                        string.Empty,
                        true);

                    await target.GetFile("test.cs");

                    // Simulate calling GetFile with a file that's not yet been initialized
                    // while doing the Update.
                    service.WhenForAnyArgs(x => x.Diff(null, null, null, null))
                        .Do(_ => target.GetFile("other.cs").Forget());

                    await target.Update(pullRequest);
                }
            }
        }

        static IPullRequestReviewCommentModel CreateComment(string diffHunk, string body = "Comment")
        {
            var result = Substitute.For<IPullRequestReviewCommentModel>();
            result.Body.Returns(body);
            result.DiffHunk.Returns(diffHunk);
            result.Path.Returns(FilePath);
            result.OriginalCommitId.Returns("ORIG");
            result.OriginalPosition.Returns(1);
            return result;
        }

        static IPullRequestModel CreatePullRequest(params IPullRequestReviewCommentModel[] comments)
        {
            var changedFile1 = Substitute.For<IPullRequestFileModel>();
            changedFile1.FileName.Returns("test.cs");
            var changedFile2 = Substitute.For<IPullRequestFileModel>();
            changedFile2.FileName.Returns("other.cs");

            var result = Substitute.For<IPullRequestModel>();
            result.Number.Returns(PullRequestNumber);
            result.Base.Returns(new GitReferenceModel("BASE", "master", "BASE_SHA", RepoUrl));
            result.Head.Returns(new GitReferenceModel("HEAD", "pr", "HEAD_SHA", RepoUrl));
            result.ChangedFiles.Returns(new[] { changedFile1, changedFile2 });
            result.ReviewComments.Returns(comments);

            return result;
        }

        static IRepository CreateRepository()
        {
            var result = Substitute.For<IRepository>();
            var branch = Substitute.For<Branch>();
            var commit = Substitute.For<Commit>();
            commit.Sha.Returns("BRANCH_TIP");
            branch.Tip.Returns(commit);
            result.Head.Returns(branch);
            return result;
        }

        static IPullRequestSessionService CreateService(FakeDiffService diffService)
        {
            var result = Substitute.For<IPullRequestSessionService>();
            result.Diff(
                Arg.Any<ILocalRepositoryModel>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<byte[]>())
                .Returns(i => diffService.Diff(
                    null,
                    i.ArgAt<string>(1),
                    i.ArgAt<string>(2),
                    i.ArgAt<byte[]>(3)));
            result.GetTipSha(Arg.Any<ILocalRepositoryModel>()).Returns("BRANCH_TIP");
            return result;
        }
    }
}
