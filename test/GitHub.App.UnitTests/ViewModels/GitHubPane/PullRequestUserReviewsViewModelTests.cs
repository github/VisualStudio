using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Primitives;
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
        public async Task InitializeAsync_Loads_User_Async()
        {
            var modelSerivce = Substitute.For<IModelService>();

            var target = CreateTarget();

            await InitializeAsync(target);

            Assert.That(target.User.Login, Is.EqualTo(AuthorLogin));
        }

        [Test]
        public async Task InitializeAsync_Creates_Reviews_Async()
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

            var user = Substitute.For<IAccount>();
            var target = CreateTarget(
                sessionManager: CreateSessionManager(pullRequest));

            await InitializeAsync(target);

            // Should load reviews by the correct author which are not Pending.
            Assert.That(target.Reviews, Has.Count.EqualTo(3));
        }

        [Test]
        public async Task Orders_Reviews_Descending_Async()
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

            var user = Substitute.For<IAccount>();
            var target = CreateTarget(
                sessionManager: CreateSessionManager(pullRequest));

            await InitializeAsync(target);

            Assert.That(target.Reviews, Is.Not.Empty);
            Assert.That(
                target.Reviews.Select(x => x.Model.SubmittedAt),
                Is.EqualTo(target.Reviews.Select(x => x.Model.SubmittedAt).OrderByDescending(x => x)));
        }

        [Test]
        public async Task First_Review_Is_Expanded_Async()
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

            var user = Substitute.For<IAccount>();
            var target = CreateTarget(
                sessionManager: CreateSessionManager(pullRequest));

            await InitializeAsync(target);

            Assert.That(target.Reviews[0].IsExpanded, Is.True);
            Assert.That(target.Reviews[1].IsExpanded, Is.False);
            Assert.That(target.Reviews[2].IsExpanded, Is.False);
        }

        async Task InitializeAsync(
            PullRequestUserReviewsViewModel target,
            LocalRepositoryModel localRepository = null,
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
                Reviews = Array.Empty<PullRequestReviewModel>(),
            };

            var session = Substitute.For<IPullRequestSession>();
            session.User.Returns(new ActorModel { Login = AuthorLogin });
            session.PullRequest.Returns(pullRequest);

            var result = Substitute.For<IPullRequestSessionManager>();
            result.GetSession(null, null, 0).ReturnsForAnyArgs(session);

            return result;
        }

        PullRequestUserReviewsViewModel CreateTarget(
            IPullRequestEditorService editorService = null,
            IPullRequestSessionManager sessionManager = null)
        {
            editorService = editorService ?? Substitute.For<IPullRequestEditorService>();
            sessionManager = sessionManager ?? CreateSessionManager();

            return new PullRequestUserReviewsViewModel(
                editorService,
                sessionManager);
        }

        LocalRepositoryModel CreateRepository(string owner = "owner", string name = "repo")
        {
            return new LocalRepositoryModel
            {
                CloneUrl = new UriString($"https://github.com/{owner}/{name}"),
                Name = name
            };
        }
    }
}
