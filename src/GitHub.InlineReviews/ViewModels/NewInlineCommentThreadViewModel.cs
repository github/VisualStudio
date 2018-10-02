using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.InlineReviews.Services;
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
        bool needsPush;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineCommentThreadViewModel"/> class.
        /// </summary>
        /// <param name="commentService">The comment service</param>
        /// <param name="session">The current PR review session.</param>
        /// <param name="file">The file being commented on.</param>
        /// <param name="lineNumber">The 0-based line number in the file.</param>
        /// <param name="leftComparisonBuffer">
        ///     True if the comment is being left on the left-hand-side of a diff; otherwise false.
        /// </param>
        public NewInlineCommentThreadViewModel(ICommentService commentService,
            IPullRequestSession session,
            IPullRequestSessionFile file,
            int lineNumber,
            bool leftComparisonBuffer)
            : base(session.User)
        {
            Guard.ArgumentNotNull(session, nameof(session));
            Guard.ArgumentNotNull(file, nameof(file));

            Session = session;
            File = file;
            LineNumber = lineNumber;
            LeftComparisonBuffer = leftComparisonBuffer;

            PostComment = ReactiveCommand.CreateFromTask<string>(
                DoPostComment,
                this.WhenAnyValue(x => x.NeedsPush, x => !x));

            EditComment = ReactiveCommand.Create<Tuple<string, string>>(_ => { });
            DeleteComment = ReactiveCommand.Create<Tuple<int, int>>(_ => { });

            var placeholder = PullRequestReviewCommentViewModel.CreatePlaceholder(session, commentService, this, CurrentUser);
            placeholder.BeginEdit.Execute();
            this.WhenAnyValue(x => x.NeedsPush).Subscribe(x => placeholder.IsReadOnly = x);
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
        /// Gets a value indicating whether the user must commit and push their changes before
        /// leaving a comment on the requested line.
        /// </summary>
        public bool NeedsPush
        {
            get { return needsPush; }
            private set { this.RaiseAndSetIfChanged(ref needsPush, value); }
        }

        async Task DoPostComment(string body)
        {
            Guard.ArgumentNotNull(body, nameof(body));

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

            await Session.PostReviewComment(
                body,
                File.CommitSha,
                File.RelativePath.Replace("\\", "/"),
                File.Diff,
                diffPosition.DiffLineNumber);
        }
    }
}
