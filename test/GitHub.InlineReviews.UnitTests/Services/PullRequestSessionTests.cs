using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.UnitTests.TestDoubles;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using NSubstitute;
using NUnit.Framework;

namespace GitHub.InlineReviews.UnitTests.Services
{
    public class PullRequestSessionTests
    {
        const int PullRequestNumber = 5;
        const string FilePath = "test.cs";

        public class TheHasPendingReviewProperty
        {
            [Test]
            public void IsFalseWithNoPendingReview()
            {
                var target = new PullRequestSession(
                    CreateRealSessionService(),
                    CreateActor(),
                    CreatePullRequest(),
                    Substitute.For<LocalRepositoryModel>(),
                    "owner",
                    true);

                Assert.That(target.HasPendingReview, Is.False);
            }

            [Test]
            public void IsFalseWithPendingReviewForOtherUser()
            {
                var currentUser = CreateActor("grokys");
                var otherUser = CreateActor("shana");
                var review = CreateReview(author: otherUser, state: PullRequestReviewState.Pending);
                var pr = CreatePullRequest(review);

                var target = new PullRequestSession(
                    CreateRealSessionService(),
                    currentUser,
                    pr,
                    Substitute.For<LocalRepositoryModel>(),
                    "owner",
                    true);

                Assert.That(target.HasPendingReview, Is.False);
            }

            [Test]
            public void IsFalseWithNonPendingReviewForCurrentUser()
            {
                var currentUser = CreateActor("grokys");
                var review = CreateReview(author: currentUser, state: PullRequestReviewState.Approved);
                var pr = CreatePullRequest(review);

                var target = new PullRequestSession(
                    CreateRealSessionService(),
                    currentUser,
                    pr,
                    Substitute.For<LocalRepositoryModel>(),
                    "owner",
                    true);

                Assert.That(target.HasPendingReview, Is.False);
            }

            [Test]
            public void IsTrueWithPendingReviewForCurrentUser()
            {
                var currentUser = CreateActor();
                var review = CreateReview(author: currentUser, state: PullRequestReviewState.Pending);
                var pr = CreatePullRequest(review);

                var target = new PullRequestSession(
                    CreateRealSessionService(),
                    currentUser,
                    pr,
                    Substitute.For<LocalRepositoryModel>(),
                    "owner",
                    true);

                Assert.That(target.HasPendingReview, Is.True);
            }

            [Test]
            public async Task IsTrueWhenRefreshedWithPendingReview()
            {
                var sessionService = CreateMockSessionService();
                var currentUser = CreateActor("grokys");
                var target = new PullRequestSession(
                    sessionService,
                    currentUser,
                    CreatePullRequest(),
                    CreateLocalRepository(),
                    "owner",
                    true);

                Assert.That(target.HasPendingReview, Is.False);

                var review = CreateReview(author: currentUser, state: PullRequestReviewState.Pending);
                UpdateReadPullRequest(sessionService, CreatePullRequest(review));
                await target.Refresh();

                Assert.That(target.HasPendingReview, Is.True);
            }

            [Test]
            public async Task IsTrueWhenStartReviewCalled()
            {
                var currentUser = CreateActor();
                var service = Substitute.For<IPullRequestSessionService>();
                var review = CreateReview(author: currentUser, state: PullRequestReviewState.Pending);
                service.CreatePendingReview(null, null).ReturnsForAnyArgs(CreatePullRequest(review));

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
                var currentUser = CreateActor();
                var review = CreateReview(author: currentUser, state: PullRequestReviewState.Pending);
                var service = Substitute.For<IPullRequestSessionService>();
                var pr = CreatePullRequest(review);

                var target = new PullRequestSession(
                    service,
                    currentUser,
                    pr,
                    Substitute.For<LocalRepositoryModel>(),
                    "owner",
                    true);

                Assert.That(target.HasPendingReview, Is.True);

                service.CancelPendingReview(null, null).ReturnsForAnyArgs(CreatePullRequest());
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
                    CreateActor(),
                    CreatePullRequest(),
                    Substitute.For<LocalRepositoryModel>(),
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
                    CreateActor(),
                    CreatePullRequest(),
                    Substitute.For<LocalRepositoryModel>(),
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
                    CreateActor(),
                    CreatePullRequest(),
                    Substitute.For<LocalRepositoryModel>(),
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
                    Arg.Any<LocalRepositoryModel>(),
                    "MERGE_BASE",
                    "HEAD_SHA",
                    FilePath).Returns(diff);

                var target = new PullRequestSession(
                    sessionService,
                    CreateActor(),
                    CreatePullRequest(),
                    Substitute.For<LocalRepositoryModel>(),
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

                var thread = CreateThread(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment");

                using (var diffService = new FakeDiffService())
                {
                    var pullRequest = CreatePullRequest(thread);
                    var service = CreateRealSessionService(diffService);

                    diffService.AddFile(FilePath, baseContents, "MERGE_BASE");
                    diffService.AddFile(FilePath, headContents, "HEAD_SHA");

                    var target = new PullRequestSession(
                        service,
                        CreateActor(),
                        pullRequest,
                        Substitute.For<LocalRepositoryModel>(),
                        "owner",
                        true);

                    var file = await target.GetFile(FilePath);
                    var inlineThread = file.InlineCommentThreads.First();
                    Assert.That(2, Is.EqualTo(inlineThread.LineNumber));
                }
            }

            [Test]
            public async Task SameNonHeadCommitShasReturnSameFiles()
            {
                var target = new PullRequestSession(
                    CreateRealSessionService(),
                    CreateActor(),
                    CreatePullRequest(),
                    Substitute.For<LocalRepositoryModel>(),
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
                    CreateActor(),
                    CreatePullRequest(),
                    Substitute.For<LocalRepositoryModel>(),
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
                    CreateActor(),
                    CreatePullRequest(),
                    Substitute.For<LocalRepositoryModel>(),
                    "owner",
                    true);

                Assert.ThrowsAsync<InvalidOperationException>(async () => await target.CancelReview());
            }

            [Test]
            public async Task CallsServiceWithNodeId()
            {
                var service = Substitute.For<IPullRequestSessionService>();
                var target = CreateTargetWithPendingReview(service);

                service.CancelPendingReview(null, null).ReturnsForAnyArgs(CreatePullRequest());

                await target.CancelReview();

                await service.Received(1).CancelPendingReview(
                    Arg.Any<LocalRepositoryModel>(),
                    "review1");
            }

            [Test]
            public async Task RemovesReviewFromModel()
            {
                var service = Substitute.For<IPullRequestSessionService>();
                var target = CreateTargetWithPendingReview(service);

                service.CancelPendingReview(null, null).ReturnsForAnyArgs(CreatePullRequest());

                await target.CancelReview();

                Assert.IsEmpty(target.PullRequest.Reviews);
            }

            public static PullRequestSession CreateTargetWithPendingReview(
                IPullRequestSessionService service)
            {
                var currentUser = CreateActor();
                var review = CreateReview(
                    author: currentUser,
                    state: PullRequestReviewState.Pending,
                    comments: CreateComment());
                var pr = CreatePullRequest(review);

                return new PullRequestSession(
                    service,
                    currentUser,
                    pr,
                    Substitute.For<LocalRepositoryModel>(),
                    "owner",
                    true);
            }
        }

        public class ThePostReviewMethod
        {
            [Test]
            public async Task PostsToCorrectForkWithNoPendingReview()
            {
                var service = CreateMockSessionService();
                var target = CreateTarget(service, "fork", "owner", false);

                service.PostReview(null, null, null, null, 0).ReturnsForAnyArgs(CreatePullRequest());
                await target.PostReview("New Review", Octokit.PullRequestReviewEvent.Approve);

                await service.Received(1).PostReview(
                    target.LocalRepository,
                    "pr1",
                    "HEAD_SHA",
                    "New Review",
                    Octokit.PullRequestReviewEvent.Approve);
            }

            [Test]
            public async Task PostsToCorrectForkWithPendingReview()
            {
                var service = CreateMockSessionService();
                var target = CreateTarget(service, "fork", "owner", true);

                service.SubmitPendingReview(null, null, null, 0).ReturnsForAnyArgs(CreatePullRequest());
                await target.PostReview("New Review", Octokit.PullRequestReviewEvent.RequestChanges);

                await service.Received(1).SubmitPendingReview(
                    target.LocalRepository,
                    "pendingReviewId",
                    "New Review",
                    Octokit.PullRequestReviewEvent.RequestChanges);
            }
        }

        public class ThePostReviewCommentMethod
        {
            [Test]
            public async Task PostsToCorrectForkWithNoPendingReview()
            {
                var service = CreateMockSessionService();
                var target = CreateTarget(service, "fork", "owner", false);

                service.PostStandaloneReviewComment(null, null, null, null, null, 0).ReturnsForAnyArgs(CreatePullRequest());
                await target.PostReviewComment("New Comment", "COMMIT_ID", "file.cs", Array.Empty<DiffChunk>(), 1);

                await service.Received(1).PostStandaloneReviewComment(
                    target.LocalRepository,
                    "pr1",
                    "New Comment",
                    "COMMIT_ID",
                    "file.cs",
                    1);
            }

            [Test]
            public async Task PostsReplyToCorrectForkWithNoPendingReview()
            {
                var service = CreateMockSessionService();
                var target = CreateTarget(service, "fork", "owner", false);

                service.PostStandaloneReviewCommentReply(null, null, null, null).ReturnsForAnyArgs(CreatePullRequest());
                await target.PostReviewComment("New Comment", "node1");

                await service.Received(1).PostStandaloneReviewCommentReply(
                    target.LocalRepository,
                    "pr1",
                    "New Comment",
                    "node1");
            }

            [Test]
            public async Task PostsToCorrectForkWithPendingReview()
            {
                var service = CreateMockSessionService();
                var target = CreateTarget(service, "fork", "owner", true);

                service.PostPendingReviewComment(null, null, null, null, null, 0).ReturnsForAnyArgs(CreatePullRequest());
                await target.PostReviewComment("New Comment", "COMMIT_ID", "file.cs", Array.Empty<DiffChunk>(), 1);

                await service.Received(1).PostPendingReviewComment(
                    target.LocalRepository,
                    "pendingReviewId",
                    "New Comment",
                    "COMMIT_ID",
                    "file.cs",
                    1);
            }

            [Test]
            public async Task PostsReplyToCorrectForkWithPendingReview()
            {
                var service = CreateMockSessionService();
                var target = CreateTarget(service, "fork", "owner", true);

                service.PostPendingReviewCommentReply(null, null, null, null).ReturnsForAnyArgs(CreatePullRequest());
                await target.PostReviewComment("New Comment", "node1");

                await service.Received(1).PostPendingReviewCommentReply(
                    target.LocalRepository,
                    "pendingReviewId",
                    "New Comment",
                    "node1");
            }
        }

        public class TheRefreshMethod
        {
            [Test]
            public async Task UpdatesThePullRequestModel()
            {
                var sessionService = CreateMockSessionService();
                var target = new PullRequestSession(
                    sessionService,
                    CreateActor(),
                    CreatePullRequest(),
                    CreateLocalRepository(),
                    "owner",
                    true);

                var newPullRequest = CreatePullRequest();
                UpdateReadPullRequest(sessionService, newPullRequest);
                await target.Refresh();

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
                var thread1 = CreateThread(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment", "Comment1");
                var thread2 = CreateThread(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment", "Comment2");

                using (var diffService = new FakeDiffService())
                {
                    var pullRequest = CreatePullRequest(thread1);
                    var service = CreateRealSessionService(diffService);

                    diffService.AddFile(FilePath, baseContents, "MERGE_BASE");
                    diffService.AddFile(FilePath, headContents, "HEAD_SHA");

                    var target = new PullRequestSession(
                        service,
                        CreateActor(),
                        pullRequest,
                        CreateLocalRepository(),
                        "owner",
                        true);

                    var file = await target.GetFile(FilePath, "HEAD");

                    Assert.That(file.InlineCommentThreads[0].Comments, Has.Count.EqualTo(1));
                    Assert.That(file.InlineCommentThreads[0].LineNumber, Is.EqualTo(2));

                    pullRequest = CreatePullRequest(thread1, thread2);
                    UpdateReadPullRequest(service, pullRequest);
                    await target.Refresh();

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

                var comment1 = CreateThread(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment", "Comment1");
                var comment2 = CreateThread(@"@@ -1,4 +1,4 @@
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
                        CreateActor(),
                        pullRequest,
                        CreateLocalRepository(),
                        "owner",
                        true);

                    var file = await target.GetFile(FilePath, "123");

                    Assert.That(file.InlineCommentThreads[0].Comments, Has.Count.EqualTo(1));
                    Assert.That(file.InlineCommentThreads[0].LineNumber, Is.EqualTo(2));

                    pullRequest = CreatePullRequest(comment1, comment2);
                    UpdateReadPullRequest(service, pullRequest);
                    await target.Refresh();

                    Assert.That(file.InlineCommentThreads[0].Comments, Has.Count.EqualTo(2));
                    Assert.That(file.InlineCommentThreads[0].LineNumber, Is.EqualTo(2));
                }
            }

            [Test]
            public async Task DoesntThrowIfGetFileCalledDuringUpdate()
            {
                var thread = CreateThread(@"@@ -1,4 +1,4 @@
 Line 1
 Line 2
-Line 3
+Line 3 with comment");

                using (var diffService = new FakeDiffService())
                {
                    var pullRequest = CreatePullRequest(thread);
                    var service = CreateRealSessionService(diffService);

                    var target = new PullRequestSession(
                        service,
                        CreateActor(),
                        pullRequest,
                        CreateLocalRepository(),
                        string.Empty,
                        true);

                    await target.GetFile("test.cs");

                    // Simulate calling GetFile with a file that's not yet been initialized
                    // while doing the Update.
                    service.WhenForAnyArgs(x => x.Diff(null, null, null, null))
                        .Do(_ => target.GetFile("other.cs").Forget());
                    UpdateReadPullRequest(service, pullRequest);

                    await target.Refresh();
                }
            }
        }

        static ActorModel CreateActor(string login = null)
        {
            return new ActorModel { Login = login ?? "Viewer" };
        }

        static PullRequestReviewCommentModel CreateComment(string body = "body")
        {
            return new PullRequestReviewCommentModel
            {
                Id = "1",
                Body = body,
            };
        }

        static PullRequestReviewModel CreateReview(
            string id = "review1",
            ActorModel author = null,
            PullRequestReviewState state = PullRequestReviewState.Approved,
            params PullRequestReviewCommentModel[] comments)
        {
            return new PullRequestReviewModel
            {
                Id = id,
                Author = author ?? CreateActor(),
                Comments = comments,
                State = state,
            };
        }

        static PullRequestReviewThreadModel CreateThread(string diffHunk, string body = "Comment")
        {
            return new PullRequestReviewThreadModel
            {
                DiffHunk = diffHunk,
                Path = FilePath,
                OriginalCommitSha = "ORIG",
                OriginalPosition = 1,
                Comments = new[]
                {
                    CreateComment(body),
                }
            };
        }

        static PullRequestDetailModel CreatePullRequest()
        {
            return CreatePullRequest(Array.Empty<PullRequestReviewModel>());
        }

        static PullRequestDetailModel CreatePullRequest(params PullRequestReviewModel[] reviews)
        {
            return new PullRequestDetailModel
            {
                Id = "pr1",
                Number = PullRequestNumber,
                BaseRefName = "BASE",
                BaseRefSha = "BASE_SHA",
                BaseRepositoryOwner = "owner",
                HeadRefName = "HEAD",
                HeadRefSha = "HEAD_SHA",
                HeadRepositoryOwner = "owner",
                ChangedFiles = new[]
                {
                    new PullRequestFileModel { FileName = FilePath },
                    new PullRequestFileModel { FileName = "other.cs" },
                },
                Threads = new[]
                {
                    new PullRequestReviewThreadModel
                    {
                        Comments = reviews.SelectMany(x => x.Comments).ToList(),
                    },
                },
                Reviews = reviews,
            };
        }

        static PullRequestDetailModel CreatePullRequest(params PullRequestReviewThreadModel[] threads)
        {
            return new PullRequestDetailModel
            {
                Id = "pr1",
                Number = PullRequestNumber,
                BaseRefName = "BASE",
                BaseRefSha = "BASE_SHA",
                BaseRepositoryOwner = "owner",
                HeadRefName = "HEAD",
                HeadRefSha = "HEAD_SHA",
                HeadRepositoryOwner = "owner",
                ChangedFiles = new[]
                {
                    new PullRequestFileModel { FileName = FilePath },
                    new PullRequestFileModel { FileName = "other.cs" },
                },
                Threads = threads,
                Reviews = new[]
                {
                    new PullRequestReviewModel
                    {
                        Author = CreateActor(),
                        Comments = threads.SelectMany(x => x.Comments).ToList(),
                    },
                }
            };
        }

        static LocalRepositoryModel CreateLocalRepository()
        {
            return new LocalRepositoryModel
            {
                CloneUrl = new UriString("https://github.com/owner/repo")
            };
        }

        static IPullRequestSessionService CreateMockSessionService()
        {
            var result = Substitute.For<IPullRequestSessionService>();
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

            result.GetTipSha(Arg.Any<LocalRepositoryModel>()).Returns("BRANCH_TIP");
            result.GetPullRequestMergeBase(Arg.Any<LocalRepositoryModel>(), Arg.Any<PullRequestDetailModel>())
                .Returns("MERGE_BASE");
            return result;
        }

        static PullRequestSession CreateTarget(
            IPullRequestSessionService service,
            string localRepositoryOwner,
            string remoteRepositoryOwner,
            bool hasPendingReview)
        {
            var repository = new LocalRepositoryModel
            {
                CloneUrl = $"https://github.com/{localRepositoryOwner}/repo",
                Name = "repo"
            };

            var pr = CreatePullRequest();
            var user = CreateActor();

            if (hasPendingReview)
            {
                pr.Reviews = new[]
                {
                        CreateReview(id: "pendingReviewId", author: user, state: PullRequestReviewState.Pending),
                    };
            }

            return new PullRequestSession(
                service,
                user,
                pr,
                repository,
                remoteRepositoryOwner,
                true);
        }

        static void UpdateReadPullRequest(IPullRequestSessionService service, PullRequestDetailModel pullRequest)
        {
            service.ReadPullRequestDetail(
                Arg.Any<HostAddress>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>()).Returns(pullRequest);
        }
    }
}
