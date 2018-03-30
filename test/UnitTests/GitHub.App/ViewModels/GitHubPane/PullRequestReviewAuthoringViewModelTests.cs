using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels.GitHubPane;
using NSubstitute;
using NUnit.Framework;

namespace UnitTests.GitHub.App.ViewModels.GitHubPane
{
    public class PullRequestReviewAuthoringViewModelTests
    {
        [Test]
        public async Task Creates_New_Pending_Review_Model()
        {
            var target = CreateTarget();

            await Initialize(target);

            Assert.That(target.Model.Id, Is.EqualTo(0));
        }

        [Test]
        public async Task Uses_Existing_Pending_Review_Model()
        {
            var review = CreateReview(12, "grokys", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest(reviews: review);

            var target = CreateTarget(model);

            await Initialize(target);

            Assert.That(target.Model.Id, Is.EqualTo(12));
        }

        [Test]
        public async Task Doesnt_Use_Non_Pending_Review_Model()
        {
            var review = CreateReview(12, "grokys", state: PullRequestReviewState.Approved);
            var model = CreatePullRequest(reviews: review);

            var target = CreateTarget(model);

            await Initialize(target);

            Assert.That(target.Model.Id, Is.EqualTo(0));
        }

        [Test]
        public async Task Doesnt_Use_Other_Users_Pending_Review_Model()
        {
            var review = CreateReview(12, "shana", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest(reviews: review);

            var target = CreateTarget(model);

            await Initialize(target);

            Assert.That(target.Model.Id, Is.EqualTo(0));
        }

        [Test]
        public async Task Body_Is_Set()
        {
            var review = CreateReview(body: "Review body");
            var model = CreatePullRequest(reviews: review);

            var target = CreateTarget(model);

            await Initialize(target);

            Assert.That(target.Body, Is.EqualTo("Review body"));
        }

        [Test]
        public async Task CanApproveRequestChanges_Is_False_When_Is_Own_PullRequest()
        {
            var review = CreateReview(12, "grokys", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest("grokys", review);

            var target = CreateTarget(model);

            await Initialize(target);

            Assert.That(target.CanApproveRequestChanges, Is.False);
        }

        [Test]
        public async Task CanApproveRequestChanges_Is_True_When_Is_Someone_Elses_PullRequest()
        {
            var review = CreateReview(12, "grokys", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest("shana", review);

            var target = CreateTarget(model);

            await Initialize(target);

            Assert.That(target.CanApproveRequestChanges, Is.True);
        }

        [Test]
        public async Task Initializes_Files()
        {
            var session = CreateSession();
            var sessionManager = CreateSessionManager(session);
            var target = CreateTarget(sessionManager: sessionManager);

            await Initialize(target);

            await target.Files.Received(1).InitializeAsync(session, Arg.Any<Func<IInlineCommentThreadModel, bool>>());
        }

        [Test]
        public async Task ReInitializes_Files_When_Session_PullRequestChanged()
        {
            var session = CreateSession();
            var sessionManager = CreateSessionManager(session);
            var target = CreateTarget(sessionManager: sessionManager);

            await Initialize(target);

            await target.Files.Received(1).InitializeAsync(session, Arg.Any<Func<IInlineCommentThreadModel, bool>>());

            RaisePullRequestChanged(session, CreatePullRequest());

            await target.Files.Received(2).InitializeAsync(session, Arg.Any<Func<IInlineCommentThreadModel, bool>>());
        }

        [Test]
        public async Task Popuplates_FileComments()
        {
            var review = CreateReview(id: 12);
            var model = CreatePullRequest(reviews: review);
            var session = CreateSession(
                "grokys",
                CreateSessionFile(
                    CreateInlineCommentThread(
                        CreateReviewComment(11)),
                    CreateInlineCommentThread(
                        CreateReviewComment(12),
                        CreateReviewComment(12))));

            var target = CreateTarget(model, session);

            await Initialize(target);

            Assert.That(target.FileComments, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task Updates_FileComments_When_Session_PullRequestChanged()
        {
            var review = CreateReview(id: 12);
            var model = CreatePullRequest(reviews: review);
            var session = CreateSession(
                "grokys",
                CreateSessionFile(
                    CreateInlineCommentThread(
                        CreateReviewComment(11)),
                    CreateInlineCommentThread(
                        CreateReviewComment(12),
                        CreateReviewComment(12))));

            var target = CreateTarget(model, session);

            await Initialize(target);

            Assert.That(target.FileComments, Has.Count.EqualTo(2));

            var newSessionFile = CreateSessionFile(
                CreateInlineCommentThread(
                    CreateReviewComment(11)),
                CreateInlineCommentThread(
                    CreateReviewComment(12)));
            session.GetAllFiles().Returns(new[] { newSessionFile });
            RaisePullRequestChanged(session, CreatePullRequest());

            Assert.That(target.FileComments, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task Updates_Model_Id_From_PendingReviewId_When_Session_PullRequestChanged()
        {
            var model = CreatePullRequest();
            var session = CreateSession(
                "grokys",
                CreateSessionFile(
                    CreateInlineCommentThread(
                        CreateReviewComment(11)),
                    CreateInlineCommentThread(
                        CreateReviewComment(12),
                        CreateReviewComment(12))));

            var target = CreateTarget(model, session);

            await Initialize(target);

            Assert.That(target.Model.Id, Is.EqualTo(0));

            session.PendingReviewId.Returns(123);
            RaisePullRequestChanged(session, model);

            Assert.That(target.Model.Id, Is.EqualTo(123));
        }

        [Test]
        public async Task Submit_Calls_Session_PostReview()
        {
            var review = CreateReview(12, "grokys", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest("shana", review);
            var session = CreateSession();

            var target = CreateTarget(model, session);

            await Initialize(target);

            target.Body = "Post review";
            target.Submit.Execute(Octokit.PullRequestReviewEvent.Approve);

            await session.Received(1).PostReview("Post review", Octokit.PullRequestReviewEvent.Approve);
        }

        [Test]
        public async Task Submit_Closes_Page()
        {
            var review = CreateReview(12, "grokys", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest("shana", review);
            var session = CreateSession();
            var closed = false;

            var target = CreateTarget(model, session);

            await Initialize(target);
            target.Body = "Post review";

            target.CloseRequested.Subscribe(_ => closed = true);
            target.Submit.Execute(Octokit.PullRequestReviewEvent.Approve);

            Assert.True(closed);
        }

        [Test]
        public async Task Cancel_Calls_Session_CancelReview_And_Closes_When_Has_Pending_Review()
        {
            var review = CreateReview(12, "grokys", state: PullRequestReviewState.Pending);
            var model = CreatePullRequest("shana", review);
            var session = CreateSession();
            var closed = false;

            var target = CreateTarget(model, session);
            await Initialize(target);

            target.CloseRequested.Subscribe(_ => closed = true);
            target.Cancel.Execute(null);

            await session.Received(1).CancelReview();
            Assert.True(closed);
        }

        [Test]
        public async Task Cancel_Just_Closes_When_Has_No_Pending_Review()
        {
            var model = CreatePullRequest("shana");
            var session = CreateSession();
            var closed = false;

            var target = CreateTarget(model, session);
            await Initialize(target);

            target.CloseRequested.Subscribe(_ => closed = true);
            target.Cancel.Execute(null);

            await session.Received(0).CancelReview();
            Assert.True(closed);
        }

        static PullRequestReviewAuthoringViewModel CreateTarget(
            IPullRequestModel model,
            IPullRequestSession session = null)
        {
            return CreateTarget(
                sessionManager: CreateSessionManager(session),
                modelServiceFactory: CreateModelServiceFactory(CreateModelService(model)));
        }

        static PullRequestReviewAuthoringViewModel CreateTarget(
            IPullRequestEditorService editorService = null,
            IPullRequestSessionManager sessionManager = null,
            IModelServiceFactory modelServiceFactory = null,
            IPullRequestFilesViewModel files = null)
        {
            editorService = editorService ?? Substitute.For<IPullRequestEditorService>();
            sessionManager = sessionManager ?? CreateSessionManager();
            modelServiceFactory = modelServiceFactory ?? CreateModelServiceFactory();
            files = files ?? Substitute.For<IPullRequestFilesViewModel>();

            return new PullRequestReviewAuthoringViewModel(
                editorService,
                sessionManager,
                modelServiceFactory,
                files);
        }

        static PullRequestReviewModel CreateReview(
            int id = 5,
            string login = "grokys",
            string body = "Review body",
            PullRequestReviewState state = PullRequestReviewState.Pending)
        {
            var user = Substitute.For<IAccount>();
            user.Login.Returns(login);

            return new PullRequestReviewModel
            {
                Id = id,
                State = state,
                User = user,
                Body = body,                
            };
        }

        static PullRequestReviewCommentModel CreateReviewComment(
            int pullRequestReviewId)
        {
            return new PullRequestReviewCommentModel
            {
                PullRequestReviewId = pullRequestReviewId,
            };
        }

        static PullRequestModel CreatePullRequest(
            string authorLogin = "grokys",
            params IPullRequestReviewModel[] reviews)
        {
            var author = Substitute.For<IAccount>();
            author.Login.Returns(authorLogin);

            var result = new PullRequestModel(
                5,
                "Pull Request",
                author,
                DateTimeOffset.Now);
            result.Reviews = reviews.ToList();
            return result;
        }

        static IPullRequestSession CreateSession(
            string userLogin = "grokys",
            params IPullRequestSessionFile[] files)
        {
            var user = Substitute.For<IAccount>();
            user.Login.Returns(userLogin);

            var result = Substitute.For<IPullRequestSession>();
            result.User.Returns(user);
            result.GetAllFiles().Returns(files);
            result.PullRequestChanged.Returns(new Subject<IPullRequestModel>());
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
            params IPullRequestReviewCommentModel[] comments)
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
            result.GetSession(null).ReturnsForAnyArgs(session);
            return result;
        }

        static IModelService CreateModelService(IPullRequestModel pullRequest = null)
        {
            pullRequest = pullRequest ?? CreatePullRequest();

            var result = Substitute.For<IModelService>();
            result.GetPullRequest(null, null, 0).ReturnsForAnyArgs(Observable.Return(pullRequest));
            return result;
        }

        static IModelServiceFactory CreateModelServiceFactory(IModelService service = null)
        {
            service = service ?? CreateModelService();

            var result = Substitute.For<IModelServiceFactory>();
            result.CreateAsync(null).ReturnsForAnyArgs(service);
            return result;
        }

        static ILocalRepositoryModel CreateLocalRepositoryModel()
        {
            var result = Substitute.For<ILocalRepositoryModel>();
            result.Owner.Returns("owner");
            result.Name.Returns("repo");
            return result;
        }

        static async Task Initialize(
            IPullRequestReviewAuthoringViewModel target,
            ILocalRepositoryModel localRepository = null)
        {
            localRepository = localRepository ?? CreateLocalRepositoryModel();

            await target.InitializeAsync(
                localRepository,
                Substitute.For<IConnection>(),
                "owner",
                "repo",
                5);
        }

        static void RaisePullRequestChanged(IPullRequestSession session, PullRequestModel newPullRequest)
        {
            ((ISubject<IPullRequestModel>)session.PullRequestChanged).OnNext(newPullRequest);
        }
    }
}
