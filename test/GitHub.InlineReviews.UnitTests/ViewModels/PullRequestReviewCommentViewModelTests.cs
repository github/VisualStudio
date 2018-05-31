using System;
using System.Reactive.Linq;
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
            ICommentThreadViewModel thread = null)
        {
            session = session ?? CreateSession();
            thread = thread ?? CreateThread();

            return new PullRequestReviewCommentViewModel(
                session,
                thread,
                Substitute.For<IActorViewModel>(),
                new PullRequestReviewCommentModel());
        }

        static IPullRequestSession CreateSession(
            bool hasPendingReview = false)
        {
            var result = Substitute.For<IPullRequestSession>();
            result.HasPendingReview.Returns(hasPendingReview);
            result.User.Returns(new ActorModel());
            return result;
        }

        static ICommentThreadViewModel CreateThread(
            bool canPost = true)
        {
            var result = Substitute.For<ICommentThreadViewModel>();
            result.PostComment.Returns(new ReactiveCommand<CommentModel>(Observable.Return(canPost), _ => null));
            return result;
        }
    }
}
