using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.InlineReviews.Models;
using GitHub.Services;

namespace GitHub.InlineReviews.ViewModels
{
    class CommentThreadViewModel
    {
        readonly IApiClient apiClient;
        readonly IPullRequestReviewSession session;

        public CommentThreadViewModel(
            IApiClient apiClient,
            IPullRequestReviewSession session,
            IEnumerable<InlineCommentModel> comments)
        {
            this.apiClient = apiClient;
            this.session = session;

            var commentViewModels = comments
                .Select(x => new CommentViewModel(this, session.User, x.Original))
                .Concat(new[]
                {
                    CommentViewModel.CreatePlaceholder(this, session.User),
                });

            Comments = new ObservableCollection<CommentViewModel>(commentViewModels);
        }

        public ObservableCollection<CommentViewModel> Comments { get; }

        public async Task<int> AddReply(string body)
        {
            var lastCommentId = Comments.Where(x => x.CommentId != 0).Last();

            var result = await apiClient.CreatePullRequestReviewComment(
                session.Repository.Owner,
                session.Repository.Name,
                session.PullRequest.Number,
                body,
                lastCommentId.CommentId);

            Comments.Add(CommentViewModel.CreatePlaceholder(this, session.User));

            return result.Id;
        }
    }
}
