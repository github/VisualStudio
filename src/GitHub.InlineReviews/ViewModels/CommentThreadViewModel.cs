using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.InlineReviews.Models;
using GitHub.Models;
using GitHub.Services;
using Octokit;

namespace GitHub.InlineReviews.ViewModels
{
    class CommentThreadViewModel
    {
        readonly IApiClient apiClient;

        public CommentThreadViewModel(
            IApiClient apiClient,
            IPullRequestReviewSession session,
            string commitSha,
            string filePath,
            int diffLine)
        {
            Guard.ArgumentNotNull(apiClient, nameof(apiClient));
            Guard.ArgumentNotNull(session, nameof(session));
            Guard.ArgumentNotNull(commitSha, nameof(commitSha));
            Guard.ArgumentNotNull(filePath, nameof(filePath));

            this.apiClient = apiClient;
            this.Session = session;
            CommitSha = commitSha;
            DiffLine = diffLine;
            FilePath = filePath;

            var placeholder = CommentViewModel.CreatePlaceholder(this, session.User);
            placeholder.BeginEdit.Execute(null);
            Comments = new ObservableCollection<CommentViewModel>();
            Comments.Add(placeholder);
        }

        public CommentThreadViewModel(
            IApiClient apiClient,
            IPullRequestReviewSession session,
            IEnumerable<InlineCommentModel> comments)
        {
            Guard.ArgumentNotNull(apiClient, nameof(apiClient));
            Guard.ArgumentNotNull(session, nameof(session));
            Guard.ArgumentNotNull(comments, nameof(comments));

            this.apiClient = apiClient;
            this.Session = session;
            CommitSha = comments.First().Original.OriginalCommitId;
            DiffLine = comments.First().Original.OriginalPosition.Value;

            var commentViewModels = comments
                .Select(x => new CommentViewModel(this, session.User, x.Original))
                .Concat(new[]
                {
                        CommentViewModel.CreatePlaceholder(this, session.User),
                });

            Comments = new ObservableCollection<CommentViewModel>(commentViewModels);
        }

        public ObservableCollection<CommentViewModel> Comments { get; }
        public string CommitSha { get; }
        public int DiffLine { get; }
        public string FilePath { get; }
        public IPullRequestReviewSession Session { get; }

        public async Task<int> AddComment(string body)
        {
            Guard.ArgumentNotNull(body, nameof(body));

            var lastComment = Comments.Where(x => x.CommentId != 0).LastOrDefault();
            var result = lastComment != null ?
                await PostReply(body, lastComment.CommentId) :
                await PostComment(body);

            Comments.Add(CommentViewModel.CreatePlaceholder(this, Session.User));

            Session.AddComment(new PullRequestReviewCommentModel
            {
                Body = result.Body,
                CommitId = result.CommitId,
                DiffHunk = result.DiffHunk,
                Id = result.Id,
                OriginalCommitId = result.OriginalCommitId,
                OriginalPosition = result.OriginalPosition,
                Path = result.Path,
                Position = result.Position,
                UpdatedAt = result.UpdatedAt,
                User = Session.User,
            });

            return result.Id;
        }

        Task<PullRequestReviewComment> PostComment(string body)
        {
            return apiClient.CreatePullRequestReviewComment(
                Session.Repository.Owner,
                Session.Repository.Name,
                Session.PullRequest.Number,
                body,
                CommitSha,
                FilePath.Replace("\\", "/"),
                DiffLine).ToTask();
        }

        Task<PullRequestReviewComment> PostReply(string body, int replyTo)
        {
            return apiClient.CreatePullRequestReviewComment(
                Session.Repository.Owner,
                Session.Repository.Name,
                Session.PullRequest.Number,
                body,
                replyTo).ToTask();
        }
    }
}
