using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.UnitTests.TestDoubles;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using LibGit2Sharp;
using NSubstitute;
using NUnit.Framework;

namespace GitHub.InlineReviews.UnitTests.Services
{
    public class PullRequestSessionTests
    {
        const int PullRequestNumber = 5;
        const string RepoUrl = "https://foo.bar/owner/repo";
        const string FilePath = "test.cs";

        public class TheGetFileMethod
        {
            [Test]
            public async Task BaseShaIsSet()
            {
                var target = new PullRequestSession(
                    CreateSessionService(),
                    Substitute.For<IAccount>(),
                    CreatePullRequest(),
                    Substitute.For<ILocalRepositoryModel>(),
                    "owner",
                    true);
                var file = await target.GetFile(FilePath);

                Assert.That("BASE_SHA", Is.SameAs(file.BaseSha));
            }

            [Test]
            public async Task CommitShaIsSet()
            {
                var target = new PullRequestSession(
                    CreateSessionService(),
                    Substitute.For<IAccount>(),
                    CreatePullRequest(),
                    Substitute.For<ILocalRepositoryModel>(),
                    "owner",
                    true);
                var file = await target.GetFile(FilePath);

                Assert.That("HEAD_SHA", Is.SameAs(file.CommitSha));
            }

            [Test]
            public async Task DiffShaIsSet()
            {
                var diff = new List<DiffChunk>();
                var sessionService = CreateSessionService();

                sessionService.Diff(
                    Arg.Any<ILocalRepositoryModel>(),
                    "MERGE_BASE",
                    "HEAD_SHA",
                    FilePath).Returns(diff);

                var target = new PullRequestSession(
                    sessionService,
                    Substitute.For<IAccount>(),
                    CreatePullRequest(),
                    Substitute.For<ILocalRepositoryModel>(),
                    "owner",
                    true);
                var file = await target.GetFile(FilePath);

                Assert.That(diff, Is.SameAs(file.Diff));
            }

            [Test]
            public async Task InlineCommentThreadsIsSet()
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
                    var service = CreateSessionService(diffService);

                    diffService.AddFile(FilePath, baseContents, "MERGE_BASE");
                    diffService.AddFile(FilePath, headContents, "HEAD_SHA");

                    var target = new PullRequestSession(
                        service,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
                        "owner",
                        true);

                    var file = await target.GetFile(FilePath);
                    var thread = file.InlineCommentThreads.First();
                    Assert.That(2, Is.EqualTo(thread.LineNumber));
                }
            }
        }

        public class ThePostReviewCommentMethod
        {
            [Test]
            public async Task PostsToCorrectFork()
            {
                var service = Substitute.For<IPullRequestSessionService>();
                var target = CreateTarget(service, "fork", "owner");

                await target.PostReviewComment("New Comment", "COMMIT_ID", "file.cs", null, 1);

                await service.Received(1).PostStandaloneReviewComment(
                    Arg.Any<ILocalRepositoryModel>(),
                    "owner",
                    Arg.Any<IAccount>(),
                    PullRequestNumber,
                    "New Comment",
                    "COMMIT_ID",
                    "file.cs",
                    1);
            }

            [Test]
            public async Task PostsReplyToCorrectFork()
            {
                var service = Substitute.For<IPullRequestSessionService>();
                var target = CreateTarget(service, "fork", "owner");

                await target.PostReviewComment("New Comment", 1, "1");

                await service.Received(1).PostReviewCommentRepy(
                    Arg.Any<ILocalRepositoryModel>(),
                    "owner",
                    Arg.Any<IAccount>(),
                    PullRequestNumber,
                    "New Comment",
                    1);
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
        }

        public class TheUpdateMethod
        {
            [Test]
            public async Task UpdatesThePullRequestModel()
            {
                var target = new PullRequestSession(
                    CreateSessionService(),
                    Substitute.For<IAccount>(),
                    CreatePullRequest(),
                    Substitute.For<ILocalRepositoryModel>(),
                    "owner",
                    true);

                var newPullRequest = CreatePullRequest();
                await target.Update(newPullRequest);

                // PullRequestModel overrides Equals such that two PRs with the same number are
                // considered equal. This was causing the PullRequest not to be updated on refresh.
                // Test that this works correctly!
                Assert.That(newPullRequest, Is.SameAs(target.PullRequest));
            }

            [Test]
            public async Task AddsNewReviewCommentToThread()
            {
                var baseContents = @"Line 1
Line 2
Line 3
Line 4";
                var headContents = @"Line 1
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
                    var service = CreateSessionService(diffService);

                    diffService.AddFile(FilePath, baseContents, "MERGE_BASE");
                    diffService.AddFile(FilePath, headContents, "HEAD_SHA");

                    var target = new PullRequestSession(
                        service,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
                        "owner",
                        true);

                    var file = await target.GetFile(FilePath);

                    Assert.That(1, Is.EqualTo(file.InlineCommentThreads[0].Comments.Count));

                    pullRequest = CreatePullRequest(comment1, comment2);
                    await target.Update(pullRequest);

                    Assert.That(2, Is.EqualTo(file.InlineCommentThreads[0].Comments.Count));
                }
            }

            [Test]
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
                    var service = CreateSessionService(diffService);

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

            result.Equals(null).ReturnsForAnyArgs(x =>
            {
                // PullRequestModel has the annoying behavior that Equals is overridden to compare
                // the pull request number, meaning that when trying to refresh, RaiseAndSetIfChanged
                // thinks the new model is the same as the old one. Make sure we replicate that
                // behavior in the mock.
                var other = x.ArgAt<object>(0) as IPullRequestModel;
                return other?.Number == result.Number;
            });

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

        static IPullRequestSessionService CreateSessionService(IDiffService diffService = null)
        {
            var result = Substitute.ForPartsOf<PullRequestSessionService>(
                Substitute.For<IGitService>(),
                Substitute.For<IGitClient>(),
                diffService ?? Substitute.For<IDiffService>(),
                Substitute.For<IApiClientFactory>(),
                Substitute.For<IGraphQLClientFactory>(),
                Substitute.For<IUsageTracker>());

            result.GetTipSha(Arg.Any<ILocalRepositoryModel>()).Returns("BRANCH_TIP");
            result.GetPullRequestMergeBase(Arg.Any<ILocalRepositoryModel>(), Arg.Any<IPullRequestModel>())
                .Returns("MERGE_BASE");
            return result;
        }
    }
}
