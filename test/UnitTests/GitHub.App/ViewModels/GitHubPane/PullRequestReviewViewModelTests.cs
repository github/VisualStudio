using System;
using System.Linq;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels.GitHubPane;
using NSubstitute;
using NUnit.Framework;

namespace UnitTests.GitHub.App.ViewModels.GitHubPane
{
    public class PullRequestReviewViewModelTests
    {
        [Test]
        public void Empty_Body_Is_Exposed_As_Null()
        {
            var pr = CreatePullRequest();
            ((PullRequestReviewModel)pr.Reviews[0]).Body = string.Empty;

            var target = CreateTarget(pullRequest: pr);

            Assert.That(target.Body, Is.Null);
        }

        [Test]
        public void Creates_FileComments_And_OutdatedComments()
        {
            var pr = CreatePullRequest();
            ((PullRequestReviewModel)pr.Reviews[0]).Body = string.Empty;

            var target = CreateTarget(pullRequest: pr);

            Assert.That(target.FileComments, Has.Count.EqualTo(2));
            Assert.That(target.OutdatedFileComments, Has.Count.EqualTo(1));
        }

        [Test]
        public void HasDetails_True_When_Has_Body()
        {
            var pr = CreatePullRequest();
            pr.ReviewComments = new IPullRequestReviewCommentModel[0];

            var target = CreateTarget(pullRequest: pr);

            Assert.That(target.HasDetails, Is.True);
        }

        [Test]
        public void HasDetails_True_When_Has_Comments()
        {
            var pr = CreatePullRequest();
            ((PullRequestReviewModel)pr.Reviews[0]).Body = string.Empty;

            var target = CreateTarget(pullRequest: pr);

            Assert.That(target.HasDetails, Is.True);
        }

        [Test]
        public void HasDetails_False_When_Has_No_Body_Or_Comments()
        {
            var pr = CreatePullRequest();
            ((PullRequestReviewModel)pr.Reviews[0]).Body = string.Empty;
            pr.ReviewComments = new IPullRequestReviewCommentModel[0];

            var target = CreateTarget(pullRequest: pr);

            Assert.That(target.HasDetails, Is.False);
        }

        PullRequestReviewViewModel CreateTarget(
            IPullRequestEditorService editorService = null,
            IPullRequestSession session = null,
            IPullRequestModel pullRequest = null,
            IPullRequestReviewModel model = null)
        {
            editorService = editorService ?? Substitute.For<IPullRequestEditorService>();
            session = session ?? Substitute.For<IPullRequestSession>();
            pullRequest = pullRequest ?? CreatePullRequest();
            model = model ?? pullRequest.Reviews[0];

            return new PullRequestReviewViewModel(
                editorService,
                session,
                pullRequest,
                model);
        }

        private PullRequestModel CreatePullRequest(
            int number = 5,
            string title = "Pull Request Title",
            string body = "Pull Request Body",
            IAccount author = null,
            DateTimeOffset? createdAt = null)
        {
            author = author ?? Substitute.For<IAccount>();
            createdAt = createdAt ?? DateTimeOffset.Now;

            return new PullRequestModel(number, title, author, createdAt.Value)
            {
                Body = body,
                Reviews = new[]
                {
                    new PullRequestReviewModel
                    {
                        Id = 1,
                        Body = "Looks good to me!",
                        State = PullRequestReviewState.Approved,
                    },
                    new PullRequestReviewModel
                    {
                        Id = 2,
                        Body = "Changes please.",
                        State = PullRequestReviewState.ChangesRequested,
                    },
                },
                ReviewComments = new[]
                {
                    new PullRequestReviewCommentModel
                    {
                        Body = "I like this.",
                        PullRequestReviewId = 1,
                        Position = 10,
                    },
                    new PullRequestReviewCommentModel
                    {
                        Body = "This is good.",
                        PullRequestReviewId = 1,
                        Position = 11,
                    },
                    new PullRequestReviewCommentModel
                    {
                        Body = "Fine, but outdated.",
                        PullRequestReviewId = 1,
                        Position = null,
                    },
                    new PullRequestReviewCommentModel
                    {
                        Body = "Not great.",
                        PullRequestReviewId = 2,
                        Position = 20,
                    },
                    new PullRequestReviewCommentModel
                    {
                        Body = "This sucks.",
                        PullRequestReviewId = 2,
                        Position = 21,
                    },
                    new PullRequestReviewCommentModel
                    {
                        Body = "Bad and old.",
                        PullRequestReviewId = 2,
                        Position = null,
                    },
                }
            };
        }
    }
}
