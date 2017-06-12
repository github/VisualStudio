using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// A new inline comment thread that is being authored.
    /// </summary>
    public class NewInlineCommentThreadViewModel : CommentThreadViewModel
    {
        readonly IApiClient apiClient;
        readonly Subject<Unit> finished = new Subject<Unit>();
        bool needsPush;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineCommentThreadViewModel"/> class.
        /// </summary>
        /// <param name="apiClient">The API client to use to post/update comments.</param>
        /// <param name="session">The current PR review session.</param>
        /// <param name="file">The file being commented on.</param>
        /// <param name="lineNumber">The 0-based line number in the file.</param>
        /// <param name="leftComparisonBuffer">
        /// True if the comment is being left on the left-hand-side of a diff; otherwise false.
        /// </param>
        public NewInlineCommentThreadViewModel(
            IApiClient apiClient,
            IPullRequestSession session,
            IPullRequestSessionFile file,
            int lineNumber,
            bool leftComparisonBuffer)
            : base(session.User)
        {
            Guard.ArgumentNotNull(apiClient, nameof(apiClient));
            Guard.ArgumentNotNull(session, nameof(session));
            Guard.ArgumentNotNull(file, nameof(file));

            this.apiClient = apiClient;
            Session = session;
            File = file;
            LineNumber = lineNumber;
            LeftComparisonBuffer = leftComparisonBuffer;

            PostComment = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.NeedsPush, x => !x),
                DoPostComment);

            var placeholder = CommentViewModel.CreatePlaceholder(this, CurrentUser);
            placeholder.BeginEdit.Execute(null);
            Comments.Add(placeholder);

            file.WhenAnyValue(x => x.CommitSha).Subscribe(x => NeedsPush = x == null);
        }

        /// <summary>
        /// Gets the file that the comment will be left on.
        /// </summary>
        public IPullRequestSessionFile File { get; }

        /// <summary>
        /// Gets the 0-based line number in the file that the comment will be left on.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Gets a value indicating whether comment is being left on the left-hand-side of a diff.
        /// </summary>
        public bool LeftComparisonBuffer { get; }

        /// <summary>
        /// Gets the current pull request review session.
        /// </summary>
        public IPullRequestSession Session { get; }

        /// <summary>
        /// Gets an observable that is fired with a single value when a comment is sucessfully
        /// posted and therefore this is no loner a new comment thread.
        /// </summary>
        public IObservable<Unit> Finished => finished;

        /// <inheritdoc/>
        public override ReactiveCommand<ICommentModel> PostComment { get; }

        /// <summary>
        /// Gets a value indicating whether the user must commit and push their changes before
        /// leaving a comment on the requested line.
        /// </summary>
        public bool NeedsPush
        {
            get { return needsPush; }
            private set { this.RaiseAndSetIfChanged(ref needsPush, value); }
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
            var result = await apiClient.CreatePullRequestReviewComment(
                Session.Repository.Owner,
                Session.Repository.Name,
                Session.PullRequest.Number,
                body,
                File.CommitSha,
                File.RelativePath.Replace("\\", "/"),
                diffPosition.DiffLineNumber);

            var model = new PullRequestReviewCommentModel
            {
                Body = result.Body,
                CommitId = result.CommitId,
                DiffHunk = result.DiffHunk,
                Id = result.Id,
                OriginalCommitId = result.OriginalCommitId,
                OriginalPosition = result.OriginalPosition,
                Path = result.Path,
                Position = result.Position,
                CreatedAt = result.CreatedAt,
                User = Session.User,
            };

            await Session.AddComment(model);
            finished.OnNext(Unit.Default);

            return model;
        }
    }
}
