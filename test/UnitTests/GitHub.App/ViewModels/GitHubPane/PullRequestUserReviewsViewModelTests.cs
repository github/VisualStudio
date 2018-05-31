using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels.GitHubPane;
using NSubstitute;
using NUnit.Framework;

namespace UnitTests.GitHub.App.ViewModels.GitHubPane
{
    public class PullRequestUserReviewsViewModelTests
    {
        const string AuthorLogin = "grokys";

        [Test]
        public async Task InitializeAsync_Loads_User()
        {
            var modelSerivce = Substitute.For<IModelService>();
            var user = new ActorModel { Login = AuthorLogin };
            modelSerivce.GetActor(AuthorLogin).Returns(user);

            var target = CreateTarget(
                modelServiceFactory: CreateFactory(modelSerivce));

            await Initialize(target);

            Assert.That(target.User.Login, Is.EqualTo(user.Login));
        }

        [Test]
        public async Task InitializeAsync_Creates_Reviews()
        {
            var author = new ActorModel { Login = AuthorLogin };
            var anotherAuthor = new ActorModel { Login = "SomeoneElse" };

            var pullRequest = new PullRequestDetailModel
            {
                Number = 5,
                Author = author,
                Reviews = new[]
                {
                    new PullRequestReviewModel
                    {
                        Author = author,
                        State = PullRequestReviewState.Approved,
                    },
                    new PullRequestReviewModel
                    {
                        Author = author,
                        State = PullRequestReviewState.ChangesRequested,
                    },
                    new PullRequestReviewModel
                    {
                        Author = anotherAuthor,
                        State = PullRequestReviewState.Approved,
                    },
                    new PullRequestReviewModel
                    {
                        Author = author,
                        State = PullRequestReviewState.Dismissed,
                    },
                    new PullRequestReviewModel
                    {
                        Author = author,
                        State = PullRequestReviewState.Pending,
                    },
                },
            };

            var modelSerivce = Substitute.For<IModelService>();
            modelSerivce.GetActor(AuthorLogin).Returns(author);

            var user = Substitute.For<IAccount>();
            var target = CreateTarget(
                sessionManager: CreateSessionManager(pullRequest),
                modelServiceFactory: CreateFactory(modelSerivce));

            await Initialize(target);

            // Should load reviews by the correct author which are not Pending.
            Assert.That(target.Reviews, Has.Count.EqualTo(3));
        }

        [Test]
        public async Task Orders_Reviews_Descending()
        {
            var author = new ActorModel { Login = AuthorLogin };

            var pullRequest = new PullRequestDetailModel
            {
                Number = 5,
                Reviews = new[]
                {
                    new PullRequestReviewModel
                    {
                        Author = author,
                        State = PullRequestReviewState.Approved,
                        SubmittedAt = DateTimeOffset.Now - TimeSpan.FromDays(2),
                    },
                    new PullRequestReviewModel
                    {
                        Author = author,
                        State = PullRequestReviewState.ChangesRequested,
                        SubmittedAt = DateTimeOffset.Now - TimeSpan.FromDays(3),
                    },
                    new PullRequestReviewModel
                    {
                        Author = author,
                        State = PullRequestReviewState.Dismissed,
                        SubmittedAt = DateTimeOffset.Now - TimeSpan.FromDays(1),
                    },
                },
            };

            var modelSerivce = Substitute.For<IModelService>();
            modelSerivce.GetActor(AuthorLogin).Returns(author);

            var user = Substitute.For<IAccount>();
            var target = CreateTarget(
                sessionManager: CreateSessionManager(pullRequest),
                modelServiceFactory: CreateFactory(modelSerivce));

            await Initialize(target);

            Assert.That(target.Reviews, Is.Not.Empty);
            Assert.That(
                target.Reviews.Select(x => x.Model.SubmittedAt),
                Is.EqualTo(target.Reviews.Select(x => x.Model.SubmittedAt).OrderByDescending(x => x)));
        }

        [Test]
        public async Task First_Review_Is_Expanded()
        {
            var author = new ActorModel { Login = AuthorLogin };

            var pullRequest = new PullRequestDetailModel
            {
                Number = 5,
                Reviews = new[]
                {
                    new PullRequestReviewModel
                    {
                        Author = author,
                        State = PullRequestReviewState.Approved,
                    },
                    new PullRequestReviewModel
                    {
                        Author = author,
                        State = PullRequestReviewState.ChangesRequested,
                    },
                    new PullRequestReviewModel
                    {
                        Author = author,
                        State = PullRequestReviewState.Dismissed,
                    },
                },
            };

            var modelSerivce = Substitute.For<IModelService>();
            modelSerivce.GetActor(AuthorLogin).Returns(author);

            var user = Substitute.For<IAccount>();
            var target = CreateTarget(
                sessionManager: CreateSessionManager(pullRequest),
                modelServiceFactory: CreateFactory(modelSerivce));

            await Initialize(target);

            Assert.That(target.Reviews[0].IsExpanded, Is.True);
            Assert.That(target.Reviews[1].IsExpanded, Is.False);
            Assert.That(target.Reviews[2].IsExpanded, Is.False);
        }

        async Task Initialize(
            PullRequestUserReviewsViewModel target,
            ILocalRepositoryModel localRepository = null,
            IConnection connection = null,
            int pullRequestNumber = 5,
            string login = AuthorLogin)
        {
            localRepository = localRepository ?? CreateRepository();
            connection = connection ?? Substitute.For<IConnection>();

            await target.InitializeAsync(
                localRepository,
                connection,
                localRepository.Owner,
                localRepository.Name,
                pullRequestNumber,
                login);
        }

        IPullRequestSessionManager CreateSessionManager(PullRequestDetailModel pullRequest = null)
        {
            pullRequest = pullRequest ?? new PullRequestDetailModel
            {
                Reviews = new PullRequestReviewModel[0],
            };

            var session = Substitute.For<IPullRequestSession>();
            session.PullRequest.Returns(pullRequest);

            var result = Substitute.For<IPullRequestSessionManager>();
            result.GetSession(null, null, 0).ReturnsForAnyArgs(session);

            return result;
        }

        PullRequestUserReviewsViewModel CreateTarget(
            IPullRequestEditorService editorService = null,
            IPullRequestSessionManager sessionManager = null,
            IModelServiceFactory modelServiceFactory = null)
        {
            editorService = editorService ?? Substitute.For<IPullRequestEditorService>();
            sessionManager = sessionManager ?? CreateSessionManager();
            modelServiceFactory = modelServiceFactory ?? Substitute.For<IModelServiceFactory>();

            return new PullRequestUserReviewsViewModel(
                editorService,
                sessionManager,
                modelServiceFactory);
        }

        IModelServiceFactory CreateFactory(IModelService modelService)
        {
            var result = Substitute.For<IModelServiceFactory>();
            result.CreateAsync(null).ReturnsForAnyArgs(modelService);
            return result;
        }

        ILocalRepositoryModel CreateRepository(string owner = "owner", string name = "repo")
        {
            var result = Substitute.For<ILocalRepositoryModel>();
            result.Owner.Returns(owner);
            result.Name.Returns(name);
            return result;
        }
    }
}
