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
            pr.Reviews[0].Body = string.Empty;

            var target = CreateTarget(pullRequest: pr);

            Assert.That(target.Body, Is.Null);
        }

        [Test]
        public void Creates_FileComments_And_OutdatedComments()
        {
            var pr = CreatePullRequest();
            pr.Reviews[0].Body = string.Empty;

            var target = CreateTarget(pullRequest: pr);

            Assert.That(target.FileComments, Has.Count.EqualTo(2));
            Assert.That(target.OutdatedFileComments, Has.Count.EqualTo(1));
        }

        [Test]
        public void HasDetails_True_When_Has_Body()
        {
            var pr = CreatePullRequest();
            var target = CreateTarget(pullRequest: pr);

            Assert.That(target.HasDetails, Is.True);
        }

        [Test]
        public void HasDetails_True_When_Has_Comments()
        {
            var pr = CreatePullRequest();
            pr.Reviews[0].Body = string.Empty;

            var target = CreateTarget(pullRequest: pr);

            Assert.That(target.HasDetails, Is.True);
        }

        [Test]
        public void HasDetails_False_When_Has_No_Body_Or_Comments()
        {
            var pr = CreatePullRequest();
            var review = pr.Reviews[0];

            review.Body = string.Empty;
            review.Comments = Array.Empty<PullRequestReviewCommentModel>();

            var target = CreateTarget(pullRequest: pr);

            Assert.That(target.HasDetails, Is.False);
        }

        PullRequestReviewViewModel CreateTarget(
            IPullRequestEditorService editorService = null,
            IPullRequestSession session = null,
            PullRequestDetailModel pullRequest = null,
            PullRequestReviewModel model = null)
        {
            editorService = editorService ?? Substitute.For<IPullRequestEditorService>();
            session = session ?? Substitute.For<IPullRequestSession>();
            pullRequest = pullRequest ?? CreatePullRequest();
            model = model ?? pullRequest.Reviews[0];

            return new PullRequestReviewViewModel(
                editorService,
                session,
                model);
        }

        private PullRequestDetailModel CreatePullRequest(
            int number = 5,
            string title = "Pull Request Title",
            string body = "Pull Request Body",
            ActorModel author = null)
        {
            var thread1 = new PullRequestReviewThreadModel
            {
                Position = 10
            };

            return new PullRequestDetailModel
            {
                Number = number,
                Title = title,
                Author = author ?? new ActorModel(),
                Body = body,
                Reviews = new[]
                {
                    new PullRequestReviewModel
                    {
                        Id = "1",
                        Body = "Looks good to me!",
                        State = PullRequestReviewState.Approved,
                        Comments = new[]
                        {
                            new PullRequestReviewCommentModel
                            {
                                Body = "I like this.",
                                Thread = new PullRequestReviewThreadModel { Position = 10 },
                            },
                            new PullRequestReviewCommentModel
                            {
                                Body = "This is good.",
                                Thread = new PullRequestReviewThreadModel { Position = 11 },
                            },
                            new PullRequestReviewCommentModel
                            {
                                Body = "Fine, but outdated.",
                                Thread = new PullRequestReviewThreadModel { Position = null },
                            },
                        },
                    },
                    new PullRequestReviewModel
                    {
                        Id = "2",
                        Body = "Changes please.",
                        State = PullRequestReviewState.ChangesRequested,
                        Comments = new[]
                        {
                            new PullRequestReviewCommentModel
                            {
                                Body = "Not great.",
                                Thread = new PullRequestReviewThreadModel { Position = 20 },
                            },
                            new PullRequestReviewCommentModel
                            {
                                Body = "This sucks.",
                                Thread = new PullRequestReviewThreadModel { Position = 21 },
                            },
                            new PullRequestReviewCommentModel
                            {
                                Body = "Bad and old.",
                                Thread = new PullRequestReviewThreadModel { Position = null },
                            },
                        },
                    },
                },
            };
        }
    }
}
