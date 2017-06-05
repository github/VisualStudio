using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.UnitTests.TestDoubles;
using GitHub.Models;
using GitHub.Services;
using LibGit2Sharp;
using NSubstitute;
using Rothko;
using Xunit;

namespace GitHub.InlineReviews.UnitTests.Services
{
    public class PullRequestSessionTests
    {
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

                var pullRequest = CreatePullRequest(comment);
                var repository = CreateRepository();
                var gitService = CreateGitService(repository);
                var gitClient = CreateGitClient(repository);

                using (var diffService = new FakeDiffService())
                {
                    diffService.AddFile(FilePath, baseContents);

                    var target = new PullRequestSession(
                        Substitute.For<IOperatingSystem>(),
                        gitService,
                        gitClient,
                        diffService,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
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

                var pullRequest = CreatePullRequest(comment);
                var repository = CreateRepository();
                var gitService = CreateGitService(repository);
                var gitClient = CreateGitClient(repository);
                using (var diffService = new FakeDiffService())
                {
                    diffService.AddFile(FilePath, baseContents);

                    var os = Substitute.For<IOperatingSystem>();
                    os.File.Exists(FilePath).Returns(true);
                    os.File.ReadAllBytesAsync(FilePath).Returns(Encoding.UTF8.GetBytes(headContents));

                    var target = new PullRequestSession(
                        os,
                        gitService,
                        gitClient,
                        diffService,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
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

                var pullRequest = CreatePullRequest(comment);
                var repository = CreateRepository();
                var gitService = CreateGitService(repository);
                var gitClient = CreateGitClient(repository);

                using (var diffService = new FakeDiffService())
                {
                    diffService.AddFile(FilePath, baseContents);

                    var target = new PullRequestSession(
                        Substitute.For<IOperatingSystem>(),
                        gitService,
                        gitClient,
                        diffService,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
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

                var pullRequest = CreatePullRequest(comment);
                var repository = CreateRepository();
                var gitService = CreateGitService(repository);
                var gitClient = CreateGitClient(repository);

                using (var diffService = new FakeDiffService())
                {
                    diffService.AddFile(FilePath, baseContents);

                    var os = Substitute.For<IOperatingSystem>();
                    os.File.Exists(FilePath).Returns(true);
                    os.File.ReadAllBytesAsync(FilePath).Returns(Encoding.UTF8.GetBytes(diskContents));

                    var target = new PullRequestSession(
                        os,
                        gitService,
                        gitClient,
                        diffService,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
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

                var pullRequest = CreatePullRequest(originalComment);
                var repository = CreateRepository();
                var gitService = CreateGitService(repository);
                var gitClient = CreateGitClient(repository);

                using (var diffService = new FakeDiffService())
                {
                    diffService.AddFile(FilePath, baseContents);

                    var target = new PullRequestSession(
                        Substitute.For<IOperatingSystem>(),
                        gitService,
                        gitClient,
                        diffService,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
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

                var pullRequest = CreatePullRequest(comment1);
                var repository = CreateRepository();
                var gitService = CreateGitService(repository);
                var gitClient = CreateGitClient(repository);

                using (var diffService = new FakeDiffService())
                {
                    diffService.AddFile(FilePath, baseContents);

                    var target = new PullRequestSession(
                        Substitute.For<IOperatingSystem>(),
                        gitService,
                        gitClient,
                        diffService,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
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

            IPullRequestReviewCommentModel CreateComment(string diffHunk, string body = "Comment")
            {
                var result = Substitute.For<IPullRequestReviewCommentModel>();
                result.Body.Returns(body);
                result.DiffHunk.Returns(diffHunk);
                result.Path.Returns(FilePath);
                result.OriginalCommitId.Returns("ORIG");
                result.OriginalPosition.Returns(1);
                return result;
            }

            IGitClient CreateGitClient(IRepository repository)
            {
                var result = Substitute.For<IGitClient>();
                result.IsModified(repository, FilePath, Arg.Any<byte[]>()).Returns(false);
                return result;
            }

            IGitService CreateGitService(IRepository repository)
            {
                var result = Substitute.For<IGitService>();
                result.GetRepository(Arg.Any<string>()).Returns(repository);
                return result;
            }

            IPullRequestModel CreatePullRequest(params IPullRequestReviewCommentModel[] comments)
            {
                var changedFile = Substitute.For<IPullRequestFileModel>();
                changedFile.FileName.Returns("test.cs");

                var result = Substitute.For<IPullRequestModel>();
                result.Base.Returns(new GitReferenceModel("BASE", "master", "BASE", RepoUrl));
                result.Head.Returns(new GitReferenceModel("HEAD", "pr", "HEAD", RepoUrl));
                result.ChangedFiles.Returns(new[] { changedFile });
                result.ReviewComments.Returns(comments);

                return result;
            }

            IRepository CreateRepository()
            {
                var result = Substitute.For<IRepository>();
                var branch = Substitute.For<Branch>();
                var commit = Substitute.For<Commit>();
                branch.Tip.Returns(commit);
                result.Head.Returns(branch);
                return result;
            }

            class FakeEditorContentSource : IEditorContentSource
            {
                byte[] content;

                public FakeEditorContentSource(string content)
                {
                    SetContent(content);
                }

                public Task<byte[]> GetContent() => Task.FromResult(content);

                public void SetContent(string content)
                {
                    this.content = Encoding.UTF8.GetBytes(content);
                }
            }
        }
    }
}
