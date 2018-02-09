using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using Octokit;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// A thread of inline comments (aka Pull Request Review Comments).
    /// </summary>
    public class InlineCommentThreadViewModel : CommentThreadViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InlineCommentThreadViewModel"/> class.
        /// </summary>
        /// <param name="apiClient">The API client to use to post/update comments.</param>
        /// <param name="file">The file being commented on.</param>
        /// <param name="session">The current PR review session.</param>
        public InlineCommentThreadViewModel(
            IPullRequestSession session,
            IPullRequestSessionFile file,
            int lineNumber,
            bool leftComparisonBuffer,
            IEnumerable<IPullRequestReviewCommentModel> comments)
            : base(session.User)
        {
            Guard.ArgumentNotNull(session, nameof(session));

            Session = session;
            File = file;
            LineNumber = lineNumber;
            LeftComparisonBuffer = leftComparisonBuffer;

            PostComment = ReactiveCommand.CreateAsyncTask(
                Observable.Return(true),
                DoPostComment);

            foreach (var comment in comments)
            {
                Comments.Add(new PullRequestReviewCommentViewModel(session, this, CurrentUser, comment));
            }

            Comments.Add(PullRequestReviewCommentViewModel.CreatePlaceholder(session, this, CurrentUser));
        }

        /// <summary>
        /// Gets the file that the comment are on.
        /// </summary>
        public IPullRequestSessionFile File { get; }

        /// <summary>
        /// Gets a value indicating whether comment is being left on the left-hand-side of a diff.
        /// </summary>
        public bool LeftComparisonBuffer { get; }

        /// <summary>
        /// Gets the 0-based line number in the file that the comment thread is on.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Gets the current pull request review session.
        /// </summary>
        public IPullRequestSession Session { get; }

        /// <inheritdoc/>
        public override Uri GetCommentUrl(int id)
        {
            return new Uri(string.Format(
                CultureInfo.InvariantCulture,
                "{0}/pull/{1}#discussion_r{2}",
                Session.LocalRepository.CloneUrl.ToRepositoryUrl(),
                Session.PullRequest.Number,
                id));
        }

        async Task<ICommentModel> DoPostComment(object parameter)
        {
            Guard.ArgumentNotNull(parameter, nameof(parameter));

            var diffPosition = File.Diff
                .SelectMany(x => x.Lines)
                .FirstOrDefault(x =>
                {
                    var line = LeftComparisonBuffer ? x.OldLineNumber : x.NewLineNumber;
                    return line == LineNumber + 1;
                });

            if (diffPosition == null)
            {
                throw new InvalidOperationException("Unable to locate line in diff.");
            }

            var body = (string)parameter;
            var replyId = Comments[0].Id;
            var nodeId = Comments[0].NodeId;
            return await Session.PostReviewComment(body, File.RelativePath, diffPosition.DiffLineNumber, replyId, nodeId);
        }
    }
}
