using System;
using System.Collections.Generic;
using System.Linq;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace GitHub.InlineReviews.UnitTests.ViewModels
{
    public class InlineCommentThreadViewModelTests
    {
        [Test]
        public void CreatesComments()
        {
            var target = new InlineCommentThreadViewModel(
                Substitute.For<ICommentService>(),
                CreateSession(), Array.Empty<InlineAnnotationViewModel>(), 
                CreateComments("Comment 1", "Comment 2"));

            Assert.That(3, Is.EqualTo(target.Comments.Count));
            Assert.That(
                new[]
                {
                    "Comment 1",
                    "Comment 2",
                    string.Empty
                },
                Is.EqualTo(target.Comments.Select(x => x.Body)));

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
        public void PlaceholderCommitEnabledWhenCommentHasBody()
        {
            var target = new InlineCommentThreadViewModel(
                Substitute.For<ICommentService>(),
                CreateSession(), Array.Empty<InlineAnnotationViewModel>(), CreateComments("Comment 1"));

            Assert.That(target.Comments[1].CommitEdit.CanExecute(null), Is.False);

            target.Comments[1].Body = "Foo";
            Assert.That(target.Comments[1].CommitEdit.CanExecute(null), Is.True);
        }

        [Test]
        public void PostsCommentInReplyToCorrectComment()
        {
            var session = CreateSession();
            var target = new InlineCommentThreadViewModel(
                Substitute.For<ICommentService>(),
                session, Array.Empty<InlineAnnotationViewModel>(),
                CreateComments("Comment 1", "Comment 2"));

            target.Comments[2].Body = "New Comment";
            target.Comments[2].CommitEdit.Execute();

            session.Received(1).PostReviewComment("New Comment", "1");
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
