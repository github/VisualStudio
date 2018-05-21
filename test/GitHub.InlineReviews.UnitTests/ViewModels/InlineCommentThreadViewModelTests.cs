using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;
using GitHub.Services;
using NSubstitute;
using Octokit;
using NUnit.Framework;

namespace GitHub.InlineReviews.UnitTests.ViewModels
{
    public class InlineCommentThreadViewModelTests
    {
        [Test]
        public void CreatesComments()
        {
            var target = new InlineCommentThreadViewModel(
                Substitute.For<IPullRequestSession>(),
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
                Substitute.For<IPullRequestSession>(),
                CreateComments("Comment 1"));

            Assert.That(target.Comments[1].CommitCreate.CanExecute(null), Is.False);

            target.Comments[1].Body = "Foo";
            Assert.That(target.Comments[1].CommitCreate.CanExecute(null), Is.True);
        }

        [Test]
        public void PostsCommentInReplyToCorrectComment()
        {
            var session = CreateSession();
            var target = new InlineCommentThreadViewModel(
                session,
                CreateComments("Comment 1", "Comment 2"));

            target.Comments[2].Body = "New Comment";
            target.Comments[2].CommitCreate.Execute(null);

            session.Received(1).PostReviewComment("New Comment", 1, "node1");
        }

        IApiClient CreateApiClient()
        {
            var result = Substitute.For<IApiClient>();
            result.CreatePullRequestReviewComment(null, null, 0, null, 0)
                .ReturnsForAnyArgs(_ => Observable.Return(new PullRequestReviewComment()));
            return result;
        }

        IPullRequestReviewCommentModel CreateComment(int id, string body)
        {
            var comment = Substitute.For<IPullRequestReviewCommentModel>();
            comment.Body.Returns(body);
            comment.Id.Returns(id);
            comment.NodeId.Returns("node" + id);
            return comment;
        }

        IEnumerable<IPullRequestReviewCommentModel> CreateComments(params string[] bodies)
        {
            var id = 1;

            foreach (var body in bodies)
            {
                yield return CreateComment(id++, body);
            }
        }

        IPullRequestSession CreateSession()
        {
            var result = Substitute.For<IPullRequestSession>();
            result.RepositoryOwner.Returns("owner");
            result.LocalRepository.Name.Returns("repo");
            result.LocalRepository.Owner.Returns("shouldnt-be-used");
            result.PullRequest.Number.Returns(47);
            return result;
        }

    }
}
