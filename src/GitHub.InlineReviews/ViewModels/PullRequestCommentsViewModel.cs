using System;
using System.Collections.ObjectModel;
using System.Linq;
using GitHub.Api;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    class PullRequestCommentsViewModel : ReactiveObject, IPullRequestCommentsViewModel
    {
        readonly IApiClient apiClient;

        public PullRequestCommentsViewModel(
            IApiClient apiClient,
            IPullRequestReviewSession session)
        {
            this.apiClient = apiClient;
            Repository = session.Repository;
            Number = session.PullRequest.Number;
            Title = session.PullRequest.Title;

            Conversation = new IssueCommentThreadViewModel(Repository, Number, session.User);

            foreach (var comment in session.PullRequest.Comments)
            {
                Conversation.Comments.Add(new CommentViewModel(
                    Conversation,
                    session.User,
                    comment));
            }

            Conversation.AddReplyPlaceholder();

            FileComments = new ObservableCollection<IDiffCommentThreadViewModel>();

            var commentsByPath = session.PullRequest.ReviewComments.GroupBy(x => x.Path);

            foreach (var path in commentsByPath)
            {
                var commentsByPosition = path.GroupBy(x => Tuple.Create(x.CommitId, x.Position));

                foreach (var position in commentsByPosition)
                {
                    var firstComment = position.First();
                    var thread = new InlineCommentThreadViewModel(
                        apiClient,
                        session,
                        firstComment.OriginalCommitId,
                        path.Key,
                        firstComment.OriginalPosition.Value);

                    foreach (var comment in position)
                    {
                        thread.Comments.Add(new InlineCommentViewModel(
                            thread,
                            session.User,
                            comment));
                    }

                    thread.AddReplyPlaceholder();

                    FileComments.Add(new DiffCommentThreadViewModel(
                        firstComment.DiffHunk,
                        path.Key,
                        thread));
                }
            }
        }

        public IRepositoryModel Repository { get; }
        public int Number { get; }
        public string Title { get; }
        public ICommentThreadViewModel Conversation { get; }
        public ObservableCollection<IDiffCommentThreadViewModel> FileComments { get; }
    }
}
