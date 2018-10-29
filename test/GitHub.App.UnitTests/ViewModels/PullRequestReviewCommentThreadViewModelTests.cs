using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels;
using NSubstitute;
using NUnit.Framework;
using ReactiveUI.Testing;

namespace GitHub.InlineReviews.UnitTests.ViewModels
{
    public class PullRequestReviewCommentThreadViewModelTests
    {
        [Test]
        public async Task CreatesComments()
        {
            var target = await CreateTarget(
                comments: CreateComments("Comment 1", "Comment 2"));

            Assert.That(3, Is.EqualTo(target.Comments.Count));
            Assert.That(
                target.Comments.Select(x => x.Body),
                Is.EqualTo(new[]
                {
                    "Comment 1",
                    "Comment 2",
                    null,
                }));

            Assert.That(
                new[]
                {
                    CommentEditState.None,
                    CommentEditState.None,
                    CommentEditState.Placeholder,
                },
                Is.EqualTo(target.Comments.Select(x => x.EditState)));
        }

        [Test]
        public async Task PlaceholderCommitEnabledWhenCommentHasBody()
        {
            var target = await CreateTarget(
                comments: CreateComments("Comment 1"));

            Assert.That(target.Comments[1].CommitEdit.CanExecute(null), Is.False);

            target.Comments[1].Body = "Foo";
            Assert.That(target.Comments[1].CommitEdit.CanExecute(null), Is.True);
        }

        [Test]
        public async Task PostsCommentInReplyToCorrectComment()
        {
            using (TestUtils.WithScheduler(Scheduler.CurrentThread))
            {
                var session = CreateSession();
                var target = await CreateTarget(
                    session: session,
                    comments: CreateComments("Comment 1", "Comment 2"));

                target.Comments[2].Body = "New Comment";
                await target.Comments[2].CommitEdit.Execute();

                session.Received(1).PostReviewComment("New Comment", "1");
            }
        }

        async Task<PullRequestReviewCommentThreadViewModel> CreateTarget(
            IViewViewModelFactory factory = null,
            IPullRequestSession session = null,
            IPullRequestSessionFile file = null,
            PullRequestReviewModel review = null,
            IEnumerable<InlineCommentModel> comments = null)
        {
            factory = factory ?? CreateFactory();
            session = session ?? CreateSession();
            file = file ?? Substitute.For<IPullRequestSessionFile>();
            review = review ?? new PullRequestReviewModel();
            comments = comments ?? CreateComments();

            var thread = Substitute.For<IInlineCommentThreadModel>();
            thread.Comments.Returns(comments.ToList());

            var result = new PullRequestReviewCommentThreadViewModel(factory);
            await result.InitializeAsync(session, file, review, thread, true);
            return result;
        }

        InlineCommentModel CreateComment(string id, string body)
        {
            return new InlineCommentModel
            {
                Comment = new PullRequestReviewCommentModel
                {
                    Id = id,
                    Body = body,
                },
                Review = new PullRequestReviewModel(),
            };
        }

        IEnumerable<InlineCommentModel> CreateComments(params string[] bodies)
        {
            var id = 1;

            foreach (var body in bodies)
            {
                yield return CreateComment((id++).ToString(), body);
            }
        }

        IViewViewModelFactory CreateFactory()
        {
            var result = Substitute.For<IViewViewModelFactory>();
            var commentService = Substitute.For<ICommentService>();
            result.CreateViewModel<IPullRequestReviewCommentViewModel>().Returns(_ =>
                new PullRequestReviewCommentViewModel(commentService));
            return result;
        }

        IPullRequestSession CreateSession()
        {
            var result = Substitute.For<IPullRequestSession>();
            result.User.Returns(new ActorModel { Login = "Viewer" });
            result.RepositoryOwner.Returns("owner");
            result.LocalRepository.Name.Returns("repo");
            result.LocalRepository.Owner.Returns("shouldnt-be-used");
            result.PullRequest.Returns(new PullRequestDetailModel
            {
                Number = 47,
            });
            return result;
        }
    }
}
