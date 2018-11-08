using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Models.Drafts;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.ViewModels.GitHubPane;
using NSubstitute;
using NUnit.Framework;

namespace UnitTests.GitHub.App.ViewModels.GitHubPane
{
    public class PullRequestReviewAuthoringViewModelTests
    {
        [Test]
        public async Task Creates_New_Pending_Review_Model_Async()
        {
            var target = CreateTarget();

            await InitializeAsync(target);

            Assert.That(target.Model.Id, Is.Null);
        }

        [Test]
        public async Task Uses_Existing_Pending_Review_Model_Async()
        {
            var review = CreateReview("12", "grokys", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest(reviews: review);

            var target = CreateTarget(model);

            await InitializeAsync(target);

            Assert.That(target.Model.Id, Is.EqualTo("12"));
        }

        [Test]
        public async Task Doesnt_Use_Non_Pending_Review_Model_Async()
        {
            var review = CreateReview("12", "grokys", state: PullRequestReviewState.Approved);
            var model = CreatePullRequest(reviews: review);

            var target = CreateTarget(model);

            await InitializeAsync(target);

            Assert.That(target.Model.Id, Is.Null);
        }

        [Test]
        public async Task Doesnt_Use_Other_Users_Pending_Review_Model_Async()
        {
            var review = CreateReview("12", "shana", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest(reviews: review);

            var target = CreateTarget(model);

            await InitializeAsync(target);

            Assert.That(target.Model.Id, Is.Null);
        }

        [Test]
        public async Task Body_Is_Set_Async()
        {
            var review = CreateReview(body: "Review body");
            var model = CreatePullRequest(reviews: review);

            var target = CreateTarget(model);

            await InitializeAsync(target);

            Assert.That(target.Body, Is.EqualTo("Review body"));
        }

        [Test]
        public async Task CanApproveRequestChanges_Is_False_When_Is_Own_PullRequest_Async()
        {
            var review = CreateReview("12", "grokys", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest("grokys", review);

            var target = CreateTarget(model);

            await InitializeAsync(target);

            Assert.That(target.CanApproveRequestChanges, Is.False);
        }

        [Test]
        public async Task CanApproveRequestChanges_Is_True_When_Is_Someone_Elses_PullRequest_Async()
        {
            var review = CreateReview("12", "grokys", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest("shana", review);

            var target = CreateTarget(model);

            await InitializeAsync(target);

            Assert.That(target.CanApproveRequestChanges, Is.True);
        }

        [Test]
        public async Task Initializes_Files_Async()
        {
            var session = CreateSession();
            var sessionManager = CreateSessionManager(session);
            var target = CreateTarget(sessionManager: sessionManager);

            await InitializeAsync(target);

            await target.Files.Received(1).InitializeAsync(session, Arg.Any<Func<IInlineCommentThreadModel, bool>>());
        }

        [Test]
        public async Task ReInitializes_Files_When_Session_PullRequestChanged_Async()
        {
            var session = CreateSession();
            var sessionManager = CreateSessionManager(session);
            var target = CreateTarget(sessionManager: sessionManager);

            await InitializeAsync(target);

            await target.Files.Received(1).InitializeAsync(session, Arg.Any<Func<IInlineCommentThreadModel, bool>>());

            RaisePullRequestChanged(session, CreatePullRequest());

            await target.Files.Received(2).InitializeAsync(session, Arg.Any<Func<IInlineCommentThreadModel, bool>>());
        }

        [Test]
        public async Task Popuplates_FileComments_Async()
        {
            var review = CreateReview(id: "12");
            var anotherReview = CreateReview(id: "11");
            var model = CreatePullRequest(reviews: review);
            var session = CreateSession(
                "grokys",
                model,
                CreateSessionFile(
                    CreateInlineCommentThread(
                        CreateReviewComment(anotherReview)),
                    CreateInlineCommentThread(
                        CreateReviewComment(review),
                        CreateReviewComment(review))));

            var target = CreateTarget(model, session);

            await InitializeAsync(target);

            Assert.That(target.FileComments, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task Updates_FileComments_When_Session_PullRequestChanged_Async()
        {
            var review = CreateReview(id: "12");
            var anotherReview = CreateReview(id: "11");
            var model = CreatePullRequest(reviews: review);
            var session = CreateSession(
                "grokys",
                model,
                CreateSessionFile(
                    CreateInlineCommentThread(
                        CreateReviewComment(anotherReview)),
                    CreateInlineCommentThread(
                        CreateReviewComment(review),
                        CreateReviewComment(review))));

            var target = CreateTarget(model, session);

            await InitializeAsync(target);

            Assert.That(target.FileComments, Has.Count.EqualTo(2));

            var newSessionFile = CreateSessionFile(
                CreateInlineCommentThread(
                    CreateReviewComment(anotherReview)),
                CreateInlineCommentThread(
                    CreateReviewComment(review)));
            session.GetAllFiles().Returns(new[] { newSessionFile });
            RaisePullRequestChanged(session, CreatePullRequest());

            Assert.That(target.FileComments, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task Updates_Model_Id_From_PendingReviewId_When_Session_PullRequestChanged_Async()
        {
            var review = CreateReview(id: "12");
            var anotherReview = CreateReview(id: "11");
            var model = CreatePullRequest();
            var session = CreateSession(
                "grokys",
                model,
                CreateSessionFile(
                    CreateInlineCommentThread(
                        CreateReviewComment(anotherReview)),
                    CreateInlineCommentThread(
                        CreateReviewComment(review),
                        CreateReviewComment(review))));

            var target = CreateTarget(model, session);

            await InitializeAsync(target);

            Assert.That(target.Model.Id, Is.Null);

            session.PendingReviewId.Returns("123");
            RaisePullRequestChanged(session, model);

            Assert.That(target.Model.Id, Is.EqualTo("123"));
        }

        [Test]
        public async Task Approve_Calls_Session_PostReview_And_Closes_Async()
        {
            var review = CreateReview("12", "grokys", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest("shana", review);
            var session = CreateSession(model: model);
            var closed = false;

            var target = CreateTarget(model, session);

            await InitializeAsync(target);
            target.Body = "Post review";
            target.CloseRequested.Subscribe(_ => closed = true);
            await target.Approve.Execute();

            await session.Received(1).PostReview("Post review", Octokit.PullRequestReviewEvent.Approve);
            Assert.True(closed);
        }

        [Test]
        public async Task Comment_Is_Disabled_When_Has_Empty_Body_And_No_File_Comments_Async()
        {
            var review = CreateReview("12", "grokys", body: "", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest("shana", review);
            var session = CreateSession(model: model);

            var target = CreateTarget(model, session);
            await InitializeAsync(target);

            var canExecute = await target.Comment.CanExecute.Take(1);
            Assert.IsFalse(canExecute);
        }

        [Test]
        public async Task Comment_Is_Enabled_When_Has_Body_Async()
        {
            var review = CreateReview("12", "grokys", body: "", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest("shana", review);
            var session = CreateSession();

            var target = CreateTarget(model, session);
            await InitializeAsync(target);
            target.Body = "Review body";

            var canExecute = await target.Comment.CanExecute.Take(1);
            Assert.IsTrue(canExecute);
        }

        [Test]
        public async Task Comment_Is_Enabled_When_Has_File_Comments_Async()
        {
            var review = CreateReview("12", "grokys", body: "", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest("shana", review);
            var session = CreateSession(
                "grokys",
                model,
                CreateSessionFile(
                    CreateInlineCommentThread(CreateReviewComment(review))));

            var target = CreateTarget(model, session);
            await InitializeAsync(target);

            var canExecute = await target.Comment.CanExecute.Take(1);
            Assert.IsTrue(canExecute);
        }

        [Test]
        public async Task Comment_Calls_Session_PostReview_And_Closes_Async()
        {
            var review = CreateReview("12", "grokys", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest("shana", review);
            var session = CreateSession();
            var closed = false;

            var target = CreateTarget(model, session);

            await InitializeAsync(target);
            target.Body = "Post review";
            target.CloseRequested.Subscribe(_ => closed = true);
            await target.Comment.Execute();

            await session.Received(1).PostReview("Post review", Octokit.PullRequestReviewEvent.Comment);
            Assert.True(closed);
        }

        [Test]
        public async Task RequestChanges_Is_Disabled_When_Has_Empty_Body_And_No_File_RequestChangess_Async()
        {
            var review = CreateReview("12", "grokys", body: "", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest("shana", review);
            var session = CreateSession();

            var target = CreateTarget(model, session);
            await InitializeAsync(target);

            var canExecute = await target.RequestChanges.CanExecute.Take(1);
            Assert.IsFalse(canExecute);
        }

        [Test]
        public async Task RequestChanges_Is_Enabled_When_Has_Body_Async()
        {
            var review = CreateReview("12", "grokys", body: "", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest("shana", review);
            var session = CreateSession();

            var target = CreateTarget(model, session);
            await InitializeAsync(target);
            target.Body = "Review body";

            var canExecute = await target.RequestChanges.CanExecute.Take(1);
            Assert.IsTrue(canExecute);
        }

        [Test]
        public async Task RequestChanges_Is_Enabled_When_Has_File_Comments_Async()
        {
            var review = CreateReview("12", "grokys", body: "", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest("shana", review);
            var session = CreateSession(
                "grokys",
                model,
                CreateSessionFile(
                    CreateInlineCommentThread(CreateReviewComment(review))));

            var target = CreateTarget(model, session);
            await InitializeAsync(target);

            var canExecute = await target.RequestChanges.CanExecute.Take(1);
            Assert.IsTrue(canExecute);
        }

        [Test]
        public async Task RequestChanges_Calls_Session_PostReview_And_Closes_Async()
        {
            var review = CreateReview("12", "grokys", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest("shana", review);
            var session = CreateSession();
            var closed = false;

            var target = CreateTarget(model, session);

            await InitializeAsync(target);
            target.Body = "Post review";
            target.CloseRequested.Subscribe(_ => closed = true);
            await target.RequestChanges.Execute();

            await session.Received(1).PostReview("Post review", Octokit.PullRequestReviewEvent.RequestChanges);
            Assert.True(closed);
        }

        [Test]
        public async Task Cancel_Calls_Session_CancelReview_And_Closes_When_Has_Pending_Review_Async()
        {
            var review = CreateReview("12", "grokys", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest("shana", review);
            var session = CreateSession(model: model);
            var closed = false;

            var pullRequestService = Substitute.For<IPullRequestService>();
            pullRequestService.ConfirmCancelPendingReview().Returns(true);

            var target = CreateTarget(model, session, pullRequestService);
            await InitializeAsync(target);

            target.CloseRequested.Subscribe(_ => closed = true);
            await target.Cancel.Execute();

            await session.Received(1).CancelReview();
            Assert.True(closed);
        }

        [Test]
        public async Task Cancel_Just_Closes_When_Has_No_Pending_Review_Async()
        {
            var model = CreatePullRequest("shana");
            var session = CreateSession();
            var closed = false;

            var target = CreateTarget(model, session);
            await InitializeAsync(target);

            target.CloseRequested.Subscribe(_ => closed = true);
            await target.Cancel.Execute();

            await session.Received(0).CancelReview();
            Assert.True(closed);
        }

        [Test]
        public async Task Loads_Draft()
        {
            var draftStore = Substitute.For<IMessageDraftStore>();
            draftStore.GetDraft<PullRequestReviewDraft>("pr-review|https://github.com/owner/repo|5", string.Empty)
                .Returns(new PullRequestReviewDraft
                {
                    Body = "This is a review.",
                });

            var target = CreateTarget(draftStore: draftStore);
            await InitializeAsync(target);

            Assert.That(target.Body, Is.EqualTo("This is a review."));
        }

        [Test]
        public async Task Updates_Draft_When_Body_Changes()
        {
            var scheduler = new HistoricalScheduler();
            var draftStore = Substitute.For<IMessageDraftStore>();
            var target = CreateTarget(draftStore: draftStore, timerScheduler: scheduler);
            await InitializeAsync(target);

            target.Body = "Body changed.";

            await draftStore.DidNotReceiveWithAnyArgs().UpdateDraft<PullRequestReviewDraft>(null, null, null);

            scheduler.AdvanceBy(TimeSpan.FromSeconds(1));

            await draftStore.Received().UpdateDraft(
                "pr-review|https://github.com/owner/repo|5",
                string.Empty,
                Arg.Is<PullRequestReviewDraft>(x => x.Body == "Body changed."));
        }

        [Test]
        public async Task Deletes_Draft_When_Review_Approved()
        {
            var scheduler = new HistoricalScheduler();
            var draftStore = Substitute.For<IMessageDraftStore>();
            var target = CreateTarget(draftStore: draftStore, timerScheduler: scheduler);
            await InitializeAsync(target);

            await target.Approve.Execute();

            await draftStore.Received().DeleteDraft("pr-review|https://github.com/owner/repo|5", string.Empty);
        }

        [Test]
        public async Task Deletes_Draft_When_Canceled()
        {
            var scheduler = new HistoricalScheduler();
            var draftStore = Substitute.For<IMessageDraftStore>();
            var target = CreateTarget(draftStore: draftStore, timerScheduler: scheduler);
            await InitializeAsync(target);

            await target.Cancel.Execute();

            await draftStore.Received().DeleteDraft("pr-review|https://github.com/owner/repo|5", string.Empty);
        }

        static PullRequestReviewAuthoringViewModel CreateTarget(
            PullRequestDetailModel model,
            IPullRequestSession session = null,
            IPullRequestService pullRequestService = null)
        {
            session = session ?? CreateSession(model: model);

            return CreateTarget(
                pullRequestService: pullRequestService,
                sessionManager: CreateSessionManager(session));
        }

        static PullRequestReviewAuthoringViewModel CreateTarget(
            IPullRequestService pullRequestService = null,
            IPullRequestEditorService editorService = null,
            IPullRequestSessionManager sessionManager = null,
            IMessageDraftStore draftStore = null,
            IPullRequestFilesViewModel files = null,
            IScheduler timerScheduler = null)
        {
            editorService = editorService ?? Substitute.For<IPullRequestEditorService>();
            sessionManager = sessionManager ?? CreateSessionManager();
            draftStore = draftStore ?? Substitute.For<IMessageDraftStore>();
            files = files ?? Substitute.For<IPullRequestFilesViewModel>();
            timerScheduler = timerScheduler ?? DefaultScheduler.Instance;

            return new PullRequestReviewAuthoringViewModel(
                pullRequestService,
                editorService,
                sessionManager,
                draftStore,
                files,
                timerScheduler);
        }

        static PullRequestReviewModel CreateReview(
            string id = "5",
            string login = "grokys",
            string body = "Review body",
            PullRequestReviewState state = PullRequestReviewState.Pending)
        {
            return new PullRequestReviewModel
            {
                Id = id,
                State = state,
                Author = new ActorModel
                {
                    Login = login,
                },
                Body = body,
            };
        }

        static InlineCommentModel CreateReviewComment(PullRequestReviewModel review)
        {
            return new InlineCommentModel
            {
                Review = review,
                Comment = new PullRequestReviewCommentModel(),
            };
        }

        static PullRequestDetailModel CreatePullRequest(
            string authorLogin = "grokys",
            params PullRequestReviewModel[] reviews)
        {
            return new PullRequestDetailModel
            {
                Number = 5,
                Title = "Pull Request",
                Author = new ActorModel
                {
                    Login = authorLogin,
                },
                Reviews = reviews.ToList(),
            };
        }

        static PullRequestDetailModel CreatePullRequest(
            string authorLogin = "grokys",
            IEnumerable<PullRequestReviewModel> reviews = null)
        {
            return new PullRequestDetailModel
            {
                Number = 5,
                Title = "Pull Request",
                Author = new ActorModel
                {
                    Login = authorLogin,
                },
                Reviews = (reviews ?? Array.Empty<PullRequestReviewModel>()).ToList()
            };
        }

        static IPullRequestSession CreateSession(
            string userLogin = "grokys",
            PullRequestDetailModel model = null,
            params IPullRequestSessionFile[] files)
        {
            model = model ?? CreatePullRequest();

            var result = Substitute.For<IPullRequestSession>();
            result.PendingReviewId.Returns((string)null);
            result.PullRequest.Returns(model);
            result.User.Returns(new ActorModel { Login = userLogin });
            result.GetAllFiles().Returns(files);
            result.PullRequestChanged.Returns(new Subject<PullRequestDetailModel>());
            return result;
        }

        static IPullRequestSessionFile CreateSessionFile(
            params IInlineCommentThreadModel[] threads)
        {
            var result = Substitute.For<IPullRequestSessionFile>();
            result.InlineCommentThreads.Returns(threads);
            return result;
        }

        static IInlineCommentThreadModel CreateInlineCommentThread(
            params InlineCommentModel[] comments)
        {
            var result = Substitute.For<IInlineCommentThreadModel>();
            result.Comments.Returns(comments);
            return result;
        }

        static IPullRequestSessionManager CreateSessionManager(
            IPullRequestSession session = null)
        {
            session = session ?? CreateSession();

            var result = Substitute.For<IPullRequestSessionManager>();
            result.GetSession(null, null, 0).ReturnsForAnyArgs(session);
            return result;
        }

        static LocalRepositoryModel CreateLocalRepositoryModel()
        {
            var result = new LocalRepositoryModel
            {
                CloneUrl = new UriString("https://github.com/owner/repo"),
                Name = "repo"
            };
            return result;
        }

        static async Task InitializeAsync(
            IPullRequestReviewAuthoringViewModel target,
            LocalRepositoryModel localRepository = null)
        {
            localRepository = localRepository ?? CreateLocalRepositoryModel();

            await target.InitializeAsync(
                localRepository,
                Substitute.For<IConnection>(),
                "owner",
                "repo",
                5);
        }

        static void RaisePullRequestChanged(IPullRequestSession session, PullRequestDetailModel newPullRequest)
        {
            ((ISubject<PullRequestDetailModel>)session.PullRequestChanged).OnNext(newPullRequest);
        }
    }
}
