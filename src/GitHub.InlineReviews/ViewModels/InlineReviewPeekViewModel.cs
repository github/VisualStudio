using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Commands;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Factories;
using GitHub.InlineReviews.Commands;
using GitHub.InlineReviews.Peek;
using GitHub.InlineReviews.Services;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using ReactiveUI;
using Serilog;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// Represents the contents of an inline comment peek view displayed in an editor.
    /// </summary>
    public sealed class InlineReviewPeekViewModel : ReactiveObject, IDisposable
    {
        static readonly ILogger log = LogManager.ForContext<InlineReviewPeekViewModel>();
        readonly IInlineReviewPeekService peekService;
        readonly IPeekSession peekSession;
        readonly IPullRequestSessionManager sessionManager;
        readonly ICommentService commentService;
        IPullRequestSession session;
        IPullRequestSessionFile file;
        IInlineReviewViewModel thread;
        IDisposable fileSubscription;
        IDisposable sessionSubscription;
        IDisposable threadSubscription;
        ITrackingPoint triggerPoint;
        string relativePath;
        DiffSide side;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineReviewPeekViewModel"/> class.
        /// </summary>
        public InlineReviewPeekViewModel(IInlineReviewPeekService peekService,
            IPeekSession peekSession,
            IPullRequestSessionManager sessionManager,
            INextInlineCommentCommand nextCommentCommand,
            IPreviousInlineCommentCommand previousCommentCommand,
            ICommentService commentService)
        {
            Guard.ArgumentNotNull(peekService, nameof(peekService));
            Guard.ArgumentNotNull(peekSession, nameof(peekSession));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));
            Guard.ArgumentNotNull(nextCommentCommand, nameof(nextCommentCommand));
            Guard.ArgumentNotNull(previousCommentCommand, nameof(previousCommentCommand));

            this.peekService = peekService;
            this.peekSession = peekSession;
            this.sessionManager = sessionManager;
            this.commentService = commentService;
            triggerPoint = peekSession.GetTriggerPoint(peekSession.TextView.TextBuffer);

            peekSession.Dismissed += (s, e) => Dispose();

            Close = this.WhenAnyValue(x => x.Thread)
                .SelectMany(x => x is NewCommentThreadInlineReviewViewModel
                    ? x.Comments.Single().CancelEdit.SelectUnit()
                    : Observable.Never<Unit>());

            NextComment = ReactiveCommand.CreateAsyncTask(
                Observable.Return(nextCommentCommand.Enabled),
                _ => nextCommentCommand.Execute(new InlineCommentNavigationParams
                {
                    FromLine = peekService.GetLineNumber(peekSession, triggerPoint).Item1,
                }));

            PreviousComment = ReactiveCommand.CreateAsyncTask(
                Observable.Return(previousCommentCommand.Enabled),
                _ => previousCommentCommand.Execute(new InlineCommentNavigationParams
                {
                    FromLine = peekService.GetLineNumber(peekSession, triggerPoint).Item1,
                }));
        }

        /// <summary>
        /// Gets the thread of comments to display.
        /// </summary>
        public IInlineReviewViewModel Thread
        {
            get { return thread; }
            private set { this.RaiseAndSetIfChanged(ref thread, value); }
        }

        /// <summary>
        /// Gets a command which moves to the next inline comment in the file.
        /// </summary>
        public ReactiveCommand<Unit> NextComment { get; }

        /// <summary>
        /// Gets a command which moves to the previous inline comment in the file.
        /// </summary>
        public ReactiveCommand<Unit> PreviousComment { get; }

        public IObservable<Unit> Close { get; }

        public void Dispose()
        {
            threadSubscription?.Dispose();
            threadSubscription = null;
            sessionSubscription?.Dispose();
            sessionSubscription = null;
            fileSubscription?.Dispose();
            fileSubscription = null;
        }

        public async Task Initialize()
        {
            var buffer = peekSession.TextView.TextBuffer;
            var info = sessionManager.GetTextBufferInfo(buffer);

            if (info != null)
            {
                var commitSha = info.Side == DiffSide.Left ? "HEAD" : info.CommitSha;
                relativePath = info.RelativePath;
                side = info.Side ?? DiffSide.Right;
                file = await info.Session.GetFile(relativePath, commitSha);
                session = info.Session;
                await UpdateThread();
            }
            else
            {
                relativePath = sessionManager.GetRelativePath(buffer);
                side = DiffSide.Right;
                file = await sessionManager.GetLiveFile(relativePath, peekSession.TextView, buffer);
                await SessionChanged(sessionManager.CurrentSession);
                sessionSubscription = sessionManager.WhenAnyValue(x => x.CurrentSession)
                    .Skip(1)
                    .Subscribe(x => SessionChanged(x).Forget());
            }

            fileSubscription?.Dispose();
            fileSubscription = file.LinesChanged.Subscribe(LinesChanged);
        }

        async void LinesChanged(IReadOnlyList<Tuple<int, DiffSide>> lines)
        {
            try
            {
                var lineNumber = peekService.GetLineNumber(peekSession, triggerPoint).Item1;

                if (lines.Contains(Tuple.Create(lineNumber, side)))
                {
                    await UpdateThread();
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Error updating InlineCommentViewModel");
            }
        }

        async Task UpdateThread()
        {
            var placeholderBody = GetPlaceholderBodyToPreserve();

            Thread = null;
            threadSubscription?.Dispose();

            if (file == null)
                return;

            var lineAndLeftBuffer = peekService.GetLineNumber(peekSession, triggerPoint);
            var lineNumber = lineAndLeftBuffer.Item1;
            var leftBuffer = lineAndLeftBuffer.Item2;
            var thread = file.InlineCommentThreads?.FirstOrDefault(x =>
                x.LineNumber == lineNumber &&
                ((leftBuffer && x.DiffLineType == DiffChangeType.Delete) || (!leftBuffer && x.DiffLineType != DiffChangeType.Delete)));

            var annotationModels = file.InlineAnnotations?.Where(model => model.EndLine - 1 == lineNumber)
                .Select(model => new InlineAnnotationViewModel(model))
                .ToArray();

            if (thread != null)
            {
                Thread = new CommentThreadInlineReviewViewModel(commentService, session, annotationModels, thread.Comments);
            }
            else
            {
                Thread = new NewCommentThreadInlineReviewViewModel(commentService, session, annotationModels, file, lineNumber, leftBuffer);
            }

            if (!string.IsNullOrWhiteSpace(placeholderBody))
            {
                var placeholder = Thread.Comments.LastOrDefault();

                if (placeholder?.EditState == CommentEditState.Placeholder)
                {
                    await placeholder.BeginEdit.ExecuteAsync(null);
                    placeholder.Body = placeholderBody;
                }
            }
        }

        async Task SessionChanged(IPullRequestSession pullRequestSession)
        {
            this.session = pullRequestSession;

            if (pullRequestSession == null)
            {
                Thread = null;
                threadSubscription?.Dispose();
                threadSubscription = null;
                return;
            }
            else
            {
                await UpdateThread();
            }
        }

        string GetPlaceholderBodyToPreserve()
        {
            var lastComment = Thread?.Comments.LastOrDefault();

            if (lastComment?.EditState == CommentEditState.Editing)
            {
                if (!lastComment.IsSubmitting) return lastComment.Body;
            }

            return null;
        }
    }
}
