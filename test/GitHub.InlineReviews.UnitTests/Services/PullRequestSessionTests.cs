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

        public class TheHasPendingReviewProperty
        {
            [Test]
            public void IsFalseWithNoPendingReview()
            {
                var target = new PullRequestSession(
                    CreateRealSessionService(),
                    Substitute.For<IAccount>(),
                    CreatePullRequest(),
                    Substitute.For<ILocalRepositoryModel>(),
                    "owner",
                    true);

                Assert.That(target.HasPendingReview, Is.False);
            }

            [Test]
            public void IsFalseWithPendingReviewForOtherUser()
            {
                var currentUser = CreateAccount("grokys");
                var otherUser = CreateAccount("shana");
                var pr = CreatePullRequest();
                var review = CreatePullRequestReview(otherUser, PullRequestReviewState.Pending);
                pr.Reviews.Returns(new[] { review });

                var target = new PullRequestSession(
                    CreateRealSessionService(),
                    currentUser,
                    pr,
                    Substitute.For<ILocalRepositoryModel>(),
                    "owner",
                    true);

                Assert.That(target.HasPendingReview, Is.False);
            }

            [Test]
            public void IsFalseWithNonPendingReviewForCurrentUser()
            {
                var currentUser = CreateAccount("grokys");
                var pr = CreatePullRequest();
                var review = CreatePullRequestReview(currentUser, PullRequestReviewState.Approved);
                pr.Reviews.Returns(new[] { review });

                var target = new PullRequestSession(
                    CreateRealSessionService(),
                    currentUser,
                    pr,
                    Substitute.For<ILocalRepositoryModel>(),
                    "owner",
                    true);

                Assert.That(target.HasPendingReview, Is.False);
            }

            [Test]
            public void IsTrueWithPendingReviewForCurrentUser()
            {
                var currentUser = Substitute.For<IAccount>();
                var pr = CreatePullRequest();
                var review = CreatePullRequestReview(currentUser, PullRequestReviewState.Pending);
                pr.Reviews.Returns(new[] { review });

                var target = new PullRequestSession(
                    CreateRealSessionService(),
                    currentUser,
                    pr,
                    Substitute.For<ILocalRepositoryModel>(),
                    "owner",
                    true);

                Assert.That(target.HasPendingReview, Is.True);
            }

            [Test]
            public async Task IsTrueWithUpdatedWithPendingReview()
            {
                var currentUser = Substitute.For<IAccount>();
                var target = new PullRequestSession(
                    CreateRealSessionService(),
                    currentUser,
                    CreatePullRequest(),
                    Substitute.For<ILocalRepositoryModel>(),
                    "owner",
                    true);

                Assert.That(target.HasPendingReview, Is.False);

                var pr = CreatePullRequest();
                var review = CreatePullRequestReview(currentUser, PullRequestReviewState.Pending);
                pr.Reviews.Returns(new[] { review });
                await target.Update(pr);

                Assert.That(target.HasPendingReview, Is.True);
            }

            [Test]
            public async Task IsTrueWhenStartReviewCalled()
            {
                var currentUser = Substitute.For<IAccount>();
                var service = Substitute.For<IPullRequestSessionService>();
                var review = CreatePullRequestReview(currentUser, PullRequestReviewState.Pending);
                service.CreatePendingReview(null, null, null).ReturnsForAnyArgs(review);

                var target = new PullRequestSession(
                    service,
                    currentUser,
                    CreatePullRequest(),
                    CreateLocalRepository(),
                    "owner",
                    true);

                Assert.That(target.HasPendingReview, Is.False);

                await target.StartReview();

                Assert.That(target.HasPendingReview, Is.True);
            }

            [Test]
            public async Task IsFalseWhenReviewCancelled()
            {
                var currentUser = Substitute.For<IAccount>();
                var pr = CreatePullRequest();
                var review = CreatePullRequestReview(currentUser, PullRequestReviewState.Pending);
                pr.Reviews.Returns(new[] { review });

                var target = new PullRequestSession(
                    Substitute.For<IPullRequestSessionService>(),
                    currentUser,
                    pr,
                    Substitute.For<ILocalRepositoryModel>(),
                    "owner",
                    true);

                Assert.That(target.HasPendingReview, Is.True);

                await target.CancelReview();

                Assert.That(target.HasPendingReview, Is.False);
            }
        }

        public class TheGetFileMethod
        {
            [Test]
            public async Task BaseShaIsSet()
            {
                var target = new PullRequestSession(
                    CreateRealSessionService(),
                    Substitute.For<IAccount>(),
                    CreatePullRequest(),
                    Substitute.For<ILocalRepositoryModel>(),
                    "owner",
                    true);
                var file = await target.GetFile(FilePath);

                Assert.That("BASE_SHA", Is.SameAs(file.BaseSha));
            }

            [Test]
            public async Task HeadCommitShaIsSet()
            {
                var target = new PullRequestSession(
                    CreateRealSessionService(),
                    Substitute.For<IAccount>(),
                    CreatePullRequest(),
                    Substitute.For<ILocalRepositoryModel>(),
                    "owner",
                    true);
                var file = await target.GetFile(FilePath);

                Assert.That("HEAD_SHA", Is.SameAs(file.CommitSha));
                Assert.That(file.IsTrackingHead, Is.True);
            }

            [Test]
            public async Task PinnedCommitShaIsSet()
            {
                var target = new PullRequestSession(
                    CreateRealSessionService(),
                    Substitute.For<IAccount>(),
                    CreatePullRequest(),
                    Substitute.For<ILocalRepositoryModel>(),
                    "owner",
                    true);
                var file = await target.GetFile(FilePath, "123");

                Assert.That("123", Is.SameAs(file.CommitSha));
                Assert.That(file.IsTrackingHead, Is.False);
            }

            [Test]
            public async Task DiffShaIsSet()
            {
                var diff = new List<DiffChunk>();
                var sessionService = CreateRealSessionService();

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
                    var service = CreateRealSessionService(diffService);

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
            
            [Test]
            public async Task SameNonHeadCommitShasReturnSameFiles()
            {
                var target = new PullRequestSession(
                    CreateRealSessionService(),
                    Substitute.For<IAccount>(),
                    CreatePullRequest(),
                    Substitute.For<ILocalRepositoryModel>(),
                    "owner",
                    true);
                var file1 = await target.GetFile(FilePath, "123");
                var file2 = await target.GetFile(FilePath, "123");

                Assert.That(file1, Is.SameAs(file2));
            }

            [Test]
            public async Task DifferentCommitShasReturnDifferentFiles()
            {
                var target = new PullRequestSession(
                    CreateRealSessionService(),
                    Substitute.For<IAccount>(),
                    CreatePullRequest(),
                    Substitute.For<ILocalRepositoryModel>(),
                    "owner",
                    true);
                var file1 = await target.GetFile(FilePath, "123");
                var file2 = await target.GetFile(FilePath, "456");

                Assert.That(file1, Is.Not.SameAs(file2));
            }
        }

        public class TheCancelReviewMethod
        {
            [Test]
            public void ThrowsWithNoPendingReview()
            {
                var target = new PullRequestSession(
                    CreateRealSessionService(),
                    Substitute.For<IAccount>(),
                    CreatePullRequest(),
                    Substitute.For<ILocalRepositoryModel>(),
                    "owner",
                    true);

                Assert.ThrowsAsync<InvalidOperationException>(async () => await target.CancelReview());
            }

            [Test]
            public async Task CallsServiceWithNodeId()
            {
                var service = Substitute.For<IPullRequestSessionService>();
                var target = CreateTargetWithPendingReview(service);

                await target.CancelReview();

                await service.Received(1).CancelPendingReview(
                    Arg.Any<ILocalRepositoryModel>(),
                    "nodeId1");
            }

            [Test]
            public async Task RemovesReviewFromModel()
            {
                var service = Substitute.For<IPullRequestSessionService>();
                var target = CreateTargetWithPendingReview(service);

                await target.CancelReview();

                Assert.IsEmpty(target.PullRequest.Reviews);
            }

            [Test]
            public async Task RemovesCommentsFromModel()
            {
                var service = Substitute.For<IPullRequestSessionService>();
                var target = CreateTargetWithPendingReview(service);

                await target.CancelReview();

                Assert.IsEmpty(target.PullRequest.ReviewComments);
            }

            public static PullRequestSession CreateTargetWithPendingReview(
                IPullRequestSessionService service)
            {
                var currentUser = Substitute.For<IAccount>();
                var pr = CreatePullRequest();
                var review = CreatePullRequestReview(currentUser, PullRequestReviewState.Pending);
                var comment = Substitute.For<IPullRequestReviewCommentModel>();

                comment.PullRequestReviewId.Returns(1);
                pr.Reviews.Returns(new[] { review });
                pr.ReviewComments.Returns(new[] { comment });

                return new PullRequestSession(
                    service,
                    currentUser,
                    pr,
                    Substitute.For<ILocalRepositoryModel>(),
                    "owner",
                    true);
            }
        }

        public class ThePostReviewMethod
        {
            [Test]
            public async Task PostsToCorrectForkWithNoPendingReview()
            {
                var service = Substitute.For<IPullRequestSessionService>();
                var target = CreateTarget(service, "fork", "owner", false);

                await target.PostReview("New Review", Octokit.PullRequestReviewEvent.Approve);

                await service.Received(1).PostReview(
                    target.LocalRepository,
                    "owner",
                    target.User,
                    PullRequestNumber,
                    "HEAD_SHA",
                    "New Review",
                    Octokit.PullRequestReviewEvent.Approve);
            }

            [Test]
            public async Task PostsToCorrectForkWithPendingReview()
            {
                var service = Substitute.For<IPullRequestSessionService>();
                var target = CreateTarget(service, "fork", "owner", true);

                await target.PostReview("New Review", Octokit.PullRequestReviewEvent.RequestChanges);

                await service.Received(1).SubmitPendingReview(
                    target.LocalRepository,
                    target.User,
                    "pendingReviewId",
                    "New Review",
                    Octokit.PullRequestReviewEvent.RequestChanges);
            }

            [Test]
            public async Task AddsReviewToModel()
            {
                var service = Substitute.For<IPullRequestSessionService>();
                var target = CreateTarget(service, "fork", "owner", false);

                var model = await target.PostReview("New Review", Octokit.PullRequestReviewEvent.RequestChanges);

                Assert.That(target.PullRequest.Reviews.Last(), Is.SameAs(model));
            }

            [Test]
            public async Task ReplacesPendingReviewWithModel()
            {
                var service = Substitute.For<IPullRequestSessionService>();

                var target = CreateTarget(service, "fork", "owner", true);

                Assert.That(
                    target.PullRequest.Reviews.Where(x => x.State == PullRequestReviewState.Pending).Count(),
                    Is.EqualTo(1));

                var submittedReview = CreatePullRequestReview(target.User, PullRequestReviewState.Approved);
                submittedReview.NodeId.Returns("pendingReviewId");
                service.SubmitPendingReview(null, null, null, null, Octokit.PullRequestReviewEvent.Approve)
                    .ReturnsForAnyArgs(submittedReview);

                var model = await target.PostReview("New Review", Octokit.PullRequestReviewEvent.Approve);

                Assert.That(
                    target.PullRequest.Reviews.Where(x => x.State == PullRequestReviewState.Pending).Count(),
                    Is.Zero);
            }

            [Test]
            public async Task MarksAssociatedCommentsAsNonPending()
            {
                var service = Substitute.For<IPullRequestSessionService>();
                var target = CreateTarget(service, "fork", "owner", true);

                Assert.That(target.PullRequest.ReviewComments[0].IsPending, Is.True);

                var submittedReview = CreatePullRequestReview(target.User, PullRequestReviewState.Approved);
                submittedReview.NodeId.Returns("pendingReviewId");
                service.SubmitPendingReview(null, null, null, null, Octokit.PullRequestReviewEvent.Approve)
                    .ReturnsForAnyArgs(submittedReview);
                var model = await target.PostReview("New Review", Octokit.PullRequestReviewEvent.RequestChanges);

                target.PullRequest.ReviewComments[0].Received(1).IsPending = false;
            }

            PullRequestSession CreateTarget(
                IPullRequestSessionService service,
                string localRepositoryOwner,
                string remoteRepositoryOwner,
                bool hasPendingReview)
            {
                var repository = Substitute.For<ILocalRepositoryModel>();

                repository.CloneUrl.Returns(new UriString($"https://github.com/{localRepositoryOwner}/reop"));
                repository.Owner.Returns(localRepositoryOwner);
                repository.Name.Returns("repo");

                var pr = CreatePullRequest();
                var user = Substitute.For<IAccount>();

                if (hasPendingReview)
                {
                    var reviewComment = Substitute.For<IPullRequestReviewCommentModel>();
                    reviewComment.PullRequestReviewId.Returns(1);
                    reviewComment.IsPending.Returns(true);
                    pr.ReviewComments.Returns(new[] { reviewComment });

                    var review = CreatePullRequestReview(user, PullRequestReviewState.Pending);
                    review.NodeId.Returns("pendingReviewId");
                    pr.Reviews.Returns(new[] { review });
                }

                return new PullRequestSession(
                    service,
                    user,
                    pr,
                    repository,
                    remoteRepositoryOwner,
                    true);
            }
        }

        public class ThePostReviewCommentMethod
        {
            [Test]
            public async Task PostsToCorrectForkWithNoPendingReview()
            {
                var service = Substitute.For<IPullRequestSessionService>();
                var target = CreateTarget(service, "fork", "owner", false);

                await target.PostReviewComment("New Comment", "COMMIT_ID", "file.cs", new DiffChunk[0], 1);

                await service.Received(1).PostStandaloneReviewComment(
                    target.LocalRepository,
                    "owner",
                    target.User,
                    PullRequestNumber,
                    "New Comment",
                    "COMMIT_ID",
                    "file.cs",
                    1);
            }

            [Test]
            public async Task PostsReplyToCorrectForkWithNoPendingReview()
            {
                var service = Substitute.For<IPullRequestSessionService>();
                var target = CreateTarget(service, "fork", "owner", false);

                await target.PostReviewComment("New Comment", 1, "node1");

                await service.Received(1).PostStandaloneReviewCommentRepy(
                    target.LocalRepository,
                    "owner",
                    target.User,
                    PullRequestNumber,
                    "New Comment",
                    1);
            }

            [Test]
            public async Task PostsToCorrectForkWithPendingReview()
            {
                var service = Substitute.For<IPullRequestSessionService>();
                var target = CreateTarget(service, "fork", "owner", true);

                await target.PostReviewComment("New Comment", "COMMIT_ID", "file.cs", new DiffChunk[0], 1);

                await service.Received(1).PostPendingReviewComment(
                    target.LocalRepository,
                    target.User,
                    "pendingReviewId",
                    "New Comment",
                    "COMMIT_ID",
                    "file.cs",
                    1);
            }

            [Test]
            public async Task PostsReplyToCorrectForkWithPendingReview()
            {
                var service = Substitute.For<IPullRequestSessionService>();
                var target = CreateTarget(service, "fork", "owner", true);

                await target.PostReviewComment("New Comment", 1, "node1");

                await service.Received(1).PostPendingReviewCommentReply(
                    target.LocalRepository,
                    target.User,
                    "pendingReviewId",
                    "New Comment",
                    "node1");
            }

            PullRequestSession CreateTarget(
                IPullRequestSessionService service,
                string localRepositoryOwner,
                string remoteRepositoryOwner,
                bool hasPendingReview)
            {
                var repository = Substitute.For<ILocalRepositoryModel>();

                repository.CloneUrl.Returns(new UriString($"https://github.com/{localRepositoryOwner}/reop"));
                repository.Owner.Returns(localRepositoryOwner);
                repository.Name.Returns("repo");

                var pr = CreatePullRequest();
                var user = Substitute.For<IAccount>();

                if (hasPendingReview)
                {
                    var review = CreatePullRequestReview(user, PullRequestReviewState.Pending);
                    review.NodeId.Returns("pendingReviewId");
                    pr.Reviews.Returns(new[] { review });
                }

                return new PullRequestSession(
                    service,
                    user,
                    pr,
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
                    CreateRealSessionService(),
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
            public async Task AddsNewReviewCommentToThreadOnHeadFile()
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
                    var service = CreateRealSessionService(diffService);

                    diffService.AddFile(FilePath, baseContents, "MERGE_BASE");
                    diffService.AddFile(FilePath, headContents, "HEAD_SHA");

                    var target = new PullRequestSession(
                        service,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
                        "owner",
                        true);

                    var file = await target.GetFile(FilePath, "HEAD");

                    Assert.That(file.InlineCommentThreads[0].Comments, Has.Count.EqualTo(1));
                    Assert.That(file.InlineCommentThreads[0].LineNumber, Is.EqualTo(2));

                    pullRequest = CreatePullRequest(comment1, comment2);
                    await target.Update(pullRequest);

                    Assert.That(file.InlineCommentThreads[0].Comments, Has.Count.EqualTo(2));
                    Assert.That(file.InlineCommentThreads[0].LineNumber, Is.EqualTo(2));
                }
            }

            [Test]
            public async Task AddsNewReviewCommentToThreadNonHeadFile()
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
                    var service = CreateRealSessionService(diffService);

                    diffService.AddFile(FilePath, baseContents, "MERGE_BASE");
                    diffService.AddFile(FilePath, headContents, "123");

                    var target = new PullRequestSession(
                        service,
                        Substitute.For<IAccount>(),
                        pullRequest,
                        Substitute.For<ILocalRepositoryModel>(),
                        "owner",
                        true);

                    var file = await target.GetFile(FilePath, "123");

                    Assert.That(file.InlineCommentThreads[0].Comments, Has.Count.EqualTo(1));
                    Assert.That(file.InlineCommentThreads[0].LineNumber, Is.EqualTo(2));

                    pullRequest = CreatePullRequest(comment1, comment2);
                    await target.Update(pullRequest);

                    Assert.That(file.InlineCommentThreads[0].Comments, Has.Count.EqualTo(2));
                    Assert.That(file.InlineCommentThreads[0].LineNumber, Is.EqualTo(2));
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
                    var service = CreateRealSessionService(diffService);

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

        static IAccount CreateAccount(string login)
        {
            var result = Substitute.For<IAccount>();
            result.Login.Returns(login);
            return result;
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

        static IPullRequestReviewModel CreatePullRequestReview(
            IAccount author,
            PullRequestReviewState state,
            long id = 1)
        {
            var result = Substitute.For<IPullRequestReviewModel>();
            result.Id.Returns(id);
            result.NodeId.Returns("nodeId" + id);
            result.User.Returns(author);
            result.State.Returns(state);
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

        static ILocalRepositoryModel CreateLocalRepository()
        {
            var result = Substitute.For<ILocalRepositoryModel>();
            result.CloneUrl.Returns(new UriString("https://github.com/owner/repo"));
            return result;
        }

        static IPullRequestSessionService CreateRealSessionService(IDiffService diffService = null)
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
