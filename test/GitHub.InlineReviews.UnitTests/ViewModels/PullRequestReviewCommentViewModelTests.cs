using System;
using System.Reactive.Linq;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;
using GitHub.Services;
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
        }

        public class TheBeginEditProperty
        {
            [Test]
            public void CanBeExecutedForPlaceholders()
            {
                var session = CreateSession();
                var thread = CreateThread();
                var currentUser = Substitute.For<IAccount>();
                var target = PullRequestReviewCommentViewModel.CreatePlaceholder(session, thread, currentUser);
                Assert.That(target.BeginEdit.CanExecute(new object()), Is.True);
            }

            [Test]
            public void CanBeExecutedForCommentsByTheSameAuthor()
            {
                var session = CreateSession();
                var thread = CreateThread();

                var currentUser = Substitute.For<IAccount>();

                var pullRequestReviewCommentModel = Substitute.For<IPullRequestReviewCommentModel>();
                pullRequestReviewCommentModel.User.Returns(currentUser);

                currentUser.Equals(Arg.Is(currentUser)).Returns(true);

                var target = CreateTarget(session, thread, currentUser, pullRequestReviewCommentModel);
                Assert.That(target.BeginEdit.CanExecute(new object()), Is.True);
            }

            [Test]
            public void CannotBeExecutedForCommentsByAnotherAuthor()
            {
                var session = CreateSession();
                var thread = CreateThread();

                var currentUser = Substitute.For<IAccount>();
                var otherUser = Substitute.For<IAccount>();

                var pullRequestReviewCommentModel = Substitute.For<IPullRequestReviewCommentModel>();
                pullRequestReviewCommentModel.User.Returns(otherUser);

                currentUser.Equals(Arg.Is(otherUser)).Returns(false);

                var target = CreateTarget(session, thread, currentUser, pullRequestReviewCommentModel);
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
                var currentUser = Substitute.For<IAccount>();
                var target = PullRequestReviewCommentViewModel.CreatePlaceholder(session, thread, currentUser);
                Assert.That(target.Delete.CanExecute(new object()), Is.False);
            }

            [Test]
            public void CanBeExecutedForCommentsByTheSameAuthor()
            {
                var session = CreateSession();
                var thread = CreateThread();

                var currentUser = Substitute.For<IAccount>();

                var pullRequestReviewCommentModel = Substitute.For<IPullRequestReviewCommentModel>();
                pullRequestReviewCommentModel.User.Returns(currentUser);

                currentUser.Equals(Arg.Is(currentUser)).Returns(true);

                var target = CreateTarget(session, thread, currentUser, pullRequestReviewCommentModel);
                Assert.That(target.Delete.CanExecute(new object()), Is.True);
            }

            [Test]
            public void CannotBeExecutedForCommentsByAnotherAuthor()
            {
                var session = CreateSession();
                var thread = CreateThread();

                var currentUser = Substitute.For<IAccount>();
                var otherUser = Substitute.For<IAccount>();

                var pullRequestReviewCommentModel = Substitute.For<IPullRequestReviewCommentModel>();
                pullRequestReviewCommentModel.User.Returns(otherUser);

                currentUser.Equals(Arg.Is(otherUser)).Returns(false);

                var target = CreateTarget(session, thread, currentUser, pullRequestReviewCommentModel);
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
        }

        public class TheStartReviewCommand
        {
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
            ICommentThreadViewModel thread = null,
            IAccount currentUser = null,
            IPullRequestReviewCommentModel pullRequestReviewCommentModel = null
            )
        {
            session = session ?? CreateSession();
            thread = thread ?? CreateThread();

            currentUser = currentUser ?? Substitute.For<IAccount>();
            pullRequestReviewCommentModel = pullRequestReviewCommentModel ?? Substitute.For<IPullRequestReviewCommentModel>();

            return new PullRequestReviewCommentViewModel(
                session,
                thread,
                currentUser,
                pullRequestReviewCommentModel);
        }

        static IPullRequestSession CreateSession(
            bool hasPendingReview = false)
        {
            var result = Substitute.For<IPullRequestSession>();
            result.HasPendingReview.Returns(hasPendingReview);
            return result;
        }

        static ICommentThreadViewModel CreateThread(
            bool canPost = true)
        {
            var result = Substitute.For<ICommentThreadViewModel>();
            result.PostComment.Returns(new ReactiveCommand<ICommentModel>(Observable.Return(canPost), _ => null));
            return result;
        }
    }
}
