using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels;
using NSubstitute;
using NUnit.Framework;
using ReactiveUI;

namespace GitHub.InlineReviews.UnitTests.ViewModels
{
    public class PullRequestReviewCommentViewModelTests
    {
        public class TheCanStartReviewProperty
        {
            [Test]
            public void IsFalseWhenSessionHasPendingReview()
            {
                var session = CreateSession(true);
                var target = CreateTarget(session);

                Assert.That(target.CanStartReview, Is.False);
            }

            [Test]
            public void IsTrueWhenSessionHasNoPendingReview()
            {
                var session = CreateSession(false);
                var target = CreateTarget(session);

                Assert.That(target.CanStartReview, Is.True);
            }

            [Test]
            public void IsFalseWhenEditingExistingComment()
            {
                var session = CreateSession(false);
                var pullRequestReviewCommentModel = new PullRequestReviewCommentModel { Id = "1" };
                var target = CreateTarget(session, comment: pullRequestReviewCommentModel);

                Assert.That(target.CanStartReview, Is.False);
            }
        }

        public class TheBeginEditProperty
        {
            [Test]
            public void CanBeExecutedForPlaceholders()
            {
                var session = CreateSession();
                var thread = CreateThread();
                var currentUser = Substitute.For<IActorViewModel>();
                var commentService = Substitute.For<ICommentService>();
                var target = PullRequestReviewCommentViewModel.CreatePlaceholder(session, commentService, thread, currentUser);
                Assert.That(target.BeginEdit.CanExecute(new object()), Is.True);
            }

            [Test]
            public void CanBeExecutedForCommentsByTheSameAuthor()
            {
                var session = CreateSession();
                var thread = CreateThread();

                var currentUser = new ActorModel { Login = "CurrentUser" };
                var comment = new PullRequestReviewCommentModel { Author = currentUser };

                var target = CreateTarget(session, null, thread, currentUser, null, comment);
                Assert.That(target.BeginEdit.CanExecute(new object()), Is.True);
            }

            [Test]
            public void CannotBeExecutedForCommentsByAnotherAuthor()
            {
                var session = CreateSession();
                var thread = CreateThread();

                var currentUser = new ActorModel { Login = "CurrentUser" };
                var otherUser = new ActorModel { Login = "OtherUser" };
                var comment = new PullRequestReviewCommentModel { Author = otherUser };

                var target = CreateTarget(session, null, thread, currentUser, null, comment);
                Assert.That(target.BeginEdit.CanExecute(new object()), Is.False);
            }
        }

        public class TheDeleteProperty
        {
            [Test]
            public void CannotBeExecutedForPlaceholders()
            {
                var session = CreateSession();
                var thread = CreateThread();
                var currentUser = Substitute.For<IActorViewModel>();
                var commentService = Substitute.For<ICommentService>();
                var target = PullRequestReviewCommentViewModel.CreatePlaceholder(session, commentService, thread, currentUser);
                Assert.That(target.Delete.CanExecute(new object()), Is.False);
            }

            [Test]
            public void CanBeExecutedForCommentsByTheSameAuthor()
            {
                var session = CreateSession();
                var thread = CreateThread();

                var currentUser = new ActorModel { Login = "CurrentUser" };
                var comment = new PullRequestReviewCommentModel { Author = currentUser };

                var target = CreateTarget(session, null, thread, currentUser, null, comment);
                Assert.That(target.Delete.CanExecute(new object()), Is.True);
            }

            [Test]
            public void CannotBeExecutedForCommentsByAnotherAuthor()
            {
                var session = CreateSession();
                var thread = CreateThread();

                var currentUser = new ActorModel { Login = "CurrentUser" };
                var otherUser = new ActorModel { Login = "OtherUser" };
                var comment = new PullRequestReviewCommentModel { Author = otherUser };

                var target = CreateTarget(session, null, thread, currentUser, null, comment);
                Assert.That(target.Delete.CanExecute(new object()), Is.False);
            }
        }

        public class TheCommitCaptionProperty
        {
            [Test]
            public void IsAddReviewCommentWhenSessionHasPendingReview()
            {
                var session = CreateSession(true);
                var target = CreateTarget(session);

                Assert.That(target.CommitCaption, Is.EqualTo("Add review comment"));
            }

            [Test]
            public void IsAddSingleCommentWhenSessionHasNoPendingReview()
            {
                var session = CreateSession(false);
                var target = CreateTarget(session);

                Assert.That(target.CommitCaption, Is.EqualTo("Add a single comment"));
            }

            [Test]
            public void IsUpdateCommentWhenEditingExistingComment()
            {
                var session = CreateSession(false);
                var pullRequestReviewCommentModel = new PullRequestReviewCommentModel { Id = "1" };
                var target = CreateTarget(session, comment: pullRequestReviewCommentModel);

                Assert.That(target.CommitCaption, Is.EqualTo("Update comment"));
            }
        }

        public class TheStartReviewCommand
        {
            public TheStartReviewCommand()
            {
                Splat.ModeDetector.Current.SetInUnitTestRunner(true);
            }

            [Test]
            public void IsDisabledWhenSessionHasPendingReview()
            {
                var session = CreateSession(true);
                var target = CreateTarget(session);

                Assert.That(target.StartReview.CanExecute(null), Is.False);
            }

            [Test]
            public void IsDisabledWhenSessionHasNoPendingReview()
            {
                var session = CreateSession(false);
                var target = CreateTarget(session);

                Assert.That(target.StartReview.CanExecute(null), Is.False);
            }

            [Test]
            public void IsEnabledWhenSessionHasNoPendingReviewAndBodyNotEmpty()
            {
                var session = CreateSession(false);
                var target = CreateTarget(session);

                target.Body = "body";

                Assert.That(target.StartReview.CanExecute(null), Is.True);
            }

            [Test]
            public void CallsSessionStartReview()
            {
                var session = CreateSession(false);
                var target = CreateTarget(session);

                target.Body = "body";
                target.StartReview.Execute(null);

                session.Received(1).StartReview();
            }
        }

        static PullRequestReviewCommentViewModel CreateTarget(
            IPullRequestSession session = null,
            ICommentService commentService = null,
            IInlineReviewViewModel thread = null,
            ActorModel currentUser = null,
            PullRequestReviewModel review = null,
            PullRequestReviewCommentModel comment = null)
        {
            session = session ?? CreateSession();
            commentService = commentService ?? Substitute.For<ICommentService>();
            thread = thread ?? CreateThread();
            currentUser = currentUser ?? new ActorModel { Login = "CurrentUser" };
            comment = comment ?? new PullRequestReviewCommentModel();
            review = review ?? CreateReview(comment);

            return new PullRequestReviewCommentViewModel(
                session,
                commentService,
                thread,
                new ActorViewModel(currentUser),
                review,
                comment);
        }

        static IPullRequestSession CreateSession(
            bool hasPendingReview = false)
        {
            var result = Substitute.For<IPullRequestSession>();
            result.HasPendingReview.Returns(hasPendingReview);
            result.User.Returns(new ActorModel());
            return result;
        }

        static PullRequestReviewModel CreateReview(params PullRequestReviewCommentModel[] comments)
        {
            return new PullRequestReviewModel
            {
                Comments = comments,
            };
        }

        static IInlineReviewViewModel CreateThread(
            bool canPost = true)
        {
            var result = Substitute.For<IInlineReviewViewModel>();
            result.PostComment.Returns(ReactiveCommand.CreateAsyncTask(_ => Task.CompletedTask));
            return result;
        }
    }
}
