using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.UnitTests.TestDoubles;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
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

        public class ThePostReviewCommentMethod
        {
            [Fact]
            public async Task PostsToCorrectFork()
            {
                var service = Substitute.For<IPullRequestSessionService>();
                var target = CreateTarget(service, "fork", "owner");

                await target.PostReviewComment("New Comment", "COMMIT_ID", "file.cs", 1);

                await service.Received(1).PostReviewComment(
                    Arg.Any<ILocalRepositoryModel>(),
                    "owner",
                    Arg.Any<IAccount>(),
                    PullRequestNumber,
                    "New Comment",
                    "COMMIT_ID",
                    "file.cs",
                    1);
            }

            [Fact]
            public async Task PostsReplyToCorrectFork()
            {
                var service = Substitute.For<IPullRequestSessionService>();
                var target = CreateTarget(service, "fork", "owner");

                await target.PostReviewComment("New Comment", 1);

                await service.Received(1).PostReviewComment(
                    Arg.Any<ILocalRepositoryModel>(),
                    "owner",
                    Arg.Any<IAccount>(),
                    PullRequestNumber,
                    "New Comment",
                    1);
            }

            [Fact]
            public async Task UpdatesFileWithNewThread()
            {
                using (var diffService = new FakeDiffService())
                {
                    var target = await CreateTarget(diffService);
                    var file = await target.GetFile(FilePath);

                    Assert.Empty(file.InlineCommentThreads);

                    await target.PostReviewComment("New Comment", 0);

                    Assert.Equal(1, file.InlineCommentThreads.Count);
                    Assert.Equal(1, file.InlineCommentThreads[0].Comments.Count);
                    Assert.Equal("New Comment", file.InlineCommentThreads[0].Comments[0].Body);
                }
            }

            PullRequestSession CreateTarget(
                IPullRequestSessionService service,
                string localRepositoryOwner,
                string remoteRepositoryOwner)
            {
                var repository = Substitute.For<ILocalRepositoryModel>();

                repository.CloneUrl.Returns(new UriString($"https://github.com/{localRepositoryOwner}/reop"));
                repository.Owner.Returns(localRepositoryOwner);
                repository.Name.Returns("repo");

                return new PullRequestSession(
                    service,
                    Substitute.For<IAccount>(),
                    CreatePullRequest(),
                    repository,
                    remoteRepositoryOwner,
                    true);
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
                    service.WhenForAnyArgs(x => x.Diff(null, null, null, null, null))
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
            var inner = new PullRequestSessionService(
                Substitute.For<IGitService>(),
                Substitute.For<IGitClient>(),
                diffService,
                Substitute.For<IApiClientFactory>(),
                Substitute.For<IUsageTracker>());

            result.Diff(
                Arg.Any<ILocalRepositoryModel>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<byte[]>())
                .Returns(i => diffService.Diff(
                    null,
                    i.ArgAt<string>(1),
                    i.ArgAt<string>(2),
                    i.ArgAt<string>(3),
                    i.ArgAt<byte[]>(4)));
            result.GetTipSha(Arg.Any<ILocalRepositoryModel>()).Returns("BRANCH_TIP");

            var diffChunk = "@@ -1,4 +1,4 @@";

            result.BuildCommentThreads(null, null, null).ReturnsForAnyArgs(i =>
                inner.BuildCommentThreads(i.Arg<IPullRequestModel>(), i.Arg<string>(), i.Arg<IReadOnlyList<DiffChunk>>()));
            result.PostReviewComment(null, null, null, 0, null, 0).ReturnsForAnyArgs(i =>
                CreateComment(diffChunk, i.ArgAt<string>(4)));
            result.PostReviewComment(null, null, null, 0, null, null, null, 0).ReturnsForAnyArgs(i =>
                CreateComment(diffChunk, i.ArgAt<string>(4)));

            return result;
        }
    }
}
