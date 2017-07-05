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
using Xunit;

namespace GitHub.InlineReviews.UnitTests.ViewModels
{
    public class InlineCommentThreadViewModelTests
    {
        [Fact]
        public void CreatesComments()
        {
            var target = new InlineCommentThreadViewModel(
                Substitute.For<IApiClient>(),
                Substitute.For<IPullRequestSession>(),
                CreateComments("Comment 1", "Comment 2"));

            Assert.Equal(3, target.Comments.Count);
            Assert.Equal(
                new[] 
                {
                    "Comment 1",
                    "Comment 2",
                    string.Empty
                }, 
                target.Comments.Select(x => x.Body));

            Assert.Equal(
                new[]
                {
                    CommentEditState.None,
                    CommentEditState.None,
                    CommentEditState.Placeholder,
                },
                target.Comments.Select(x => x.EditState));
        }

        [Fact]
        public void PlaceholderCommitEnabledWhenCommentHasBody()
        {
            var target = new InlineCommentThreadViewModel(
                Substitute.For<IApiClient>(),
                Substitute.For<IPullRequestSession>(),
                CreateComments("Comment 1"));

            Assert.False(target.Comments[1].CommitEdit.CanExecute(null));

            target.Comments[1].Body = "Foo";
            Assert.True(target.Comments[1].CommitEdit.CanExecute(null));
        }

        [Fact]
        public void PostsCommentInReplyToCorrectComment()
        {
            var apiClient = CreateApiClient();
            var target = new InlineCommentThreadViewModel(
                apiClient,
                CreateSession(),
                CreateComments("Comment 1", "Comment 2"));

            target.Comments[2].Body = "New Comment";
            target.Comments[2].CommitEdit.Execute(null);

            apiClient.Received(1).CreatePullRequestReviewComment(
                "owner",
                "repo",
                47,
                "New Comment",
                1);
        }

        [Fact]
        public void AddsPostedCommentToSession()
        {
            var session = CreateSession();
            var target = new InlineCommentThreadViewModel(
                CreateApiClient(),
                session,
                CreateComments("Comment 1", "Comment 2"));

            target.Comments[2].Body = "New Comment";
            target.Comments[2].CommitEdit.Execute(null);

            session.Received(1).AddComment(Arg.Any<IPullRequestReviewCommentModel>());
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
            result.Repository.Owner.Returns("owner");
            result.Repository.Name.Returns("repo");
            result.PullRequest.Number.Returns(47);
            return result;
        }

    }
}
