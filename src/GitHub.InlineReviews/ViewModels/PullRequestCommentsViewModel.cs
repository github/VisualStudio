using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    class PullRequestCommentsViewModel : ReactiveObject, IPullRequestCommentsViewModel
    {
        readonly IApiClient apiClient;
        readonly IPullRequestSession session;

        public PullRequestCommentsViewModel(
            IApiClient apiClient,
            IPullRequestSession session)
        {
            this.apiClient = apiClient;
            this.session = session;

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
        }

        public IRepositoryModel Repository { get; }
        public int Number { get; }
        public string Title { get; }
        public ICommentThreadViewModel Conversation { get; }
        public IReactiveList<IDiffCommentThreadViewModel> FileComments { get; }
            = new ReactiveList<IDiffCommentThreadViewModel>();

        public async Task Initialize()
        {
            var files = await session.GetAllFiles();

            foreach (var file in files)
            {
                foreach (var thread in file.InlineCommentThreads)
                {
                    var threadViewModel = new InlineCommentThreadViewModel(
                        apiClient,
                        session,
                        thread.Comments);

                    FileComments.Add(new DiffCommentThreadViewModel(
                        ToString(thread.DiffMatch),
                        thread.LineNumber,
                        file.RelativePath,
                        threadViewModel));
                }
            }
        }

        private string ToString(IList<DiffLine> diffMatch)
        {
            var b = new StringBuilder();

            for (var i = diffMatch.Count - 1; i >= 0; --i)
            {
                b.AppendLine(diffMatch[i].Content);
            }

            return b.ToString();
        }
    }
}
