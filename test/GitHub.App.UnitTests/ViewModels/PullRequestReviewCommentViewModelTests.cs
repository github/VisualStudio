using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels;
using NSubstitute;
using NUnit.Framework;
using ReactiveUI;

namespace GitHub.App.UnitTests.ViewModels
{
    public class PullRequestReviewCommentViewModelTests
    {
        public class TheCanStartReviewProperty
        {
            [Test]
            public async Task IsFalseWhenSessionHasPendingReview()
            {
                var session = CreateSession();
                var target = await CreateTarget(
                    session: session,
                    review: CreateReview(PullRequestReviewState.Pending));

                Assert.That(target.CanStartReview, Is.False);
            }

            [Test]
            public async Task IsTrueWhenSessionHasNoPendingReview()
            {
                var session = CreateSession();
                var target = await CreateTarget(session);

                Assert.That(target.CanStartReview, Is.True);
            }

            [Test]
            public async Task IsFalseWhenEditingExistingComment()
            {
                var session = CreateSession();
                var pullRequestReviewCommentModel = new PullRequestReviewCommentModel { Id = "1" };
                var target = await CreateTarget(session, comment: pullRequestReviewCommentModel);

                Assert.That(target.CanStartReview, Is.False);
            }
        }

        public class TheBeginEditProperty
        {
            [Test]
            public async Task CanBeExecutedForPlaceholders()
            {
                var session = CreateSession();
                var thread = CreateThread();
                var currentUser = Substitute.For<IActorViewModel>();
                var commentService = Substitute.For<ICommentService>();
                var target = new PullRequestReviewCommentViewModel(commentService);

                await target.InitializeAsPlaceholderAsync(session, thread, false, false);

                Assert.That(target.BeginEdit.CanExecute(new object()), Is.True);
            }

            [Test]
            public async Task CanBeExecutedForCommentsByTheSameAuthor()
            {
                var session = CreateSession();
                var thread = CreateThread();

                var currentUser = new ActorModel { Login = "CurrentUser" };
                var comment = new PullRequestReviewCommentModel { Author = currentUser };

                var target = await CreateTarget(session, null, thread, currentUser, null, comment);
                Assert.That(target.BeginEdit.CanExecute(new object()), Is.True);
            }

            [Test]
            public async Task CannotBeExecutedForCommentsByAnotherAuthor()
            {
                var session = CreateSession();
                var thread = CreateThread();

                var currentUser = new ActorModel { Login = "CurrentUser" };
                var otherUser = new ActorModel { Login = "OtherUser" };
                var comment = new PullRequestReviewCommentModel { Author = otherUser };

                var target = await CreateTarget(session, null, thread, currentUser, null, comment);
                Assert.That(target.BeginEdit.CanExecute(new object()), Is.False);
            }
        }

        public class TheDeleteProperty
        {
            [Test]
            public async Task CannotBeExecutedForPlaceholders()
            {
                var session = CreateSession();
                var thread = CreateThread();
                var currentUser = Substitute.For<IActorViewModel>();
                var commentService = Substitute.For<ICommentService>();
                var target = new PullRequestReviewCommentViewModel(commentService);

                await target.InitializeAsPlaceholderAsync(session, thread, false, false);

                Assert.That(target.Delete.CanExecute(new object()), Is.False);
            }

            [Test]
            public async Task CanBeExecutedForCommentsByTheSameAuthor()
            {
                var session = CreateSession();
                var thread = CreateThread();

                var currentUser = new ActorModel { Login = "CurrentUser" };
                var comment = new PullRequestReviewCommentModel { Author = currentUser };

                var target = await CreateTarget(session, null, thread, currentUser, null, comment);
                Assert.That(target.Delete.CanExecute(new object()), Is.True);
            }

            [Test]
            public async Task CannotBeExecutedForCommentsByAnotherAuthor()
            {
                var session = CreateSession();
                var thread = CreateThread();

                var currentUser = new ActorModel { Login = "CurrentUser" };
                var otherUser = new ActorModel { Login = "OtherUser" };
                var comment = new PullRequestReviewCommentModel { Author = otherUser };

                var target = await CreateTarget(session, null, thread, currentUser, null, comment);
                Assert.That(target.Delete.CanExecute(new object()), Is.False);
            }
        }

        public class TheCommitCaptionProperty
        {
            [Test]
            public async Task IsAddReviewCommentWhenSessionHasPendingReview()
            {
                var session = CreateSession();
                var target = await CreateTarget(
                    session: session,
                    review: CreateReview(PullRequestReviewState.Pending));

                Assert.That(target.CommitCaption, Is.EqualTo("Add review comment"));
            }

            [Test]
            public async Task IsAddSingleCommentWhenSessionHasNoPendingReview()
            {
                var session = CreateSession();
                var target = await CreateTarget(session);

                Assert.That(target.CommitCaption, Is.EqualTo("Add a single comment"));
            }

            [Test]
            public async Task IsUpdateCommentWhenEditingExistingComment()
            {
                var session = CreateSession();
                var pullRequestReviewCommentModel = new PullRequestReviewCommentModel { Id = "1" };
                var target = await CreateTarget(session, comment: pullRequestReviewCommentModel);

                Assert.That(target.CommitCaption, Is.EqualTo("Update comment"));
            }
        }

        public class TheStartReviewCommand
        {
            [Test]
            public async Task IsDisabledWhenSessionHasPendingReview()
            {
                var session = CreateSession();
                var target = await CreateTarget(
                    session: session,
                    review: CreateReview(PullRequestReviewState.Pending));

                Assert.That(target.StartReview.CanExecute(null), Is.False);
            }

            [Test]
            public async Task IsDisabledWhenSessionHasNoPendingReview()
            {
                var session = CreateSession();
                var target = await CreateTarget(session);

                Assert.That(target.StartReview.CanExecute(null), Is.False);
            }

            [Test]
            public async Task IsEnabledWhenSessionHasNoPendingReviewAndBodyNotEmpty()
            {
                var session = CreateSession();
                var target = await CreateTarget(session);

                target.Body = "body";

                Assert.That(target.StartReview.CanExecute(null), Is.True);
            }

            [Test]
            public async Task CallsSessionStartReview()
            {
                var session = CreateSession();
                var target = await CreateTarget(session);

                target.Body = "body";
                target.StartReview.Execute();

                session.Received(1).StartReview();
            }
        }

        static async Task<PullRequestReviewCommentViewModel> CreateTarget(
            IPullRequestSession session = null,
            ICommentService commentService = null,
            ICommentThreadViewModel thread = null,
            ActorModel currentUser = null,
            PullRequestReviewModel review = null,
            PullRequestReviewCommentModel comment = null)
        {
            session = session ?? CreateSession();
            commentService = commentService ?? Substitute.For<ICommentService>();
            thread = thread ?? CreateThread();
            currentUser = currentUser ?? new ActorModel { Login = "CurrentUser" };
            comment = comment ?? new PullRequestReviewCommentModel();
            review = review ?? CreateReview(PullRequestReviewState.Approved, comment);

            var result = new PullRequestReviewCommentViewModel(commentService);
            await result.InitializeAsync(session, thread, review, comment, CommentEditState.None);
            return result;
        }

        static IPullRequestSession CreateSession()
        {
            var result = Substitute.For<IPullRequestSession>();
            result.User.Returns(new ActorModel { Login = "CurrentUser" });
            return result;
        }

        static PullRequestReviewModel CreateReview(
            PullRequestReviewState state,
            params PullRequestReviewCommentModel[] comments)
        {
            return new PullRequestReviewModel
            {
                State = state,
                Comments = comments,
            };
        }

        static ICommentThreadViewModel CreateThread(
            bool canPost = true)
        {
            var result = Substitute.For<ICommentThreadViewModel>();
            return result;
        }
    }
}
