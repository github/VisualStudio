using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.InlineReviews.Commands;
using GitHub.InlineReviews.Services;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using NLog;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// Represents the contents of an inline comment peek view displayed in an editor.
    /// </summary>
    public sealed class InlineCommentPeekViewModel : ReactiveObject, IDisposable
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly IInlineCommentPeekService peekService;
        readonly IPeekSession peekSession;
        readonly IPullRequestSessionManager sessionManager;
        IPullRequestSession session;
        IPullRequestSessionFile file;
        ICommentThreadViewModel thread;
        IDisposable fileSubscription;
        IDisposable sessionSubscription;
        IDisposable threadSubscription;
        ITrackingPoint triggerPoint;
        string relativePath;
        DiffSide side;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineCommentPeekViewModel"/> class.
        /// </summary>
        public InlineCommentPeekViewModel(
            IInlineCommentPeekService peekService,
            IPeekSession peekSession,
            IPullRequestSessionManager sessionManager,
            INextInlineCommentCommand nextCommentCommand,
            IPreviousInlineCommentCommand previousCommentCommand)
        {
            Guard.ArgumentNotNull(peekService, nameof(peekService));
            Guard.ArgumentNotNull(peekSession, nameof(peekSession));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));
            Guard.ArgumentNotNull(nextCommentCommand, nameof(nextCommentCommand));
            Guard.ArgumentNotNull(previousCommentCommand, nameof(previousCommentCommand));

            this.peekService = peekService;
            this.peekSession = peekSession;
            this.sessionManager = sessionManager;
            triggerPoint = peekSession.GetTriggerPoint(peekSession.TextView.TextBuffer);

            peekSession.Dismissed += (s, e) => Dispose();

            NextComment = ReactiveCommand.CreateAsyncTask(
                Observable.Return(nextCommentCommand.IsEnabled),
                _ => nextCommentCommand.Execute(new InlineCommentNavigationParams
                {
                    FromLine = peekService.GetLineNumber(peekSession, triggerPoint).Item1,
                }));

            PreviousComment = ReactiveCommand.CreateAsyncTask(
                Observable.Return(previousCommentCommand.IsEnabled),
                _ => previousCommentCommand.Execute(new InlineCommentNavigationParams
                {
                    FromLine = peekService.GetLineNumber(peekSession, triggerPoint).Item1,
                }));
        }

        /// <summary>
        /// Gets the thread of comments to display.
        /// </summary>
        public ICommentThreadViewModel Thread
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
                relativePath = info.RelativePath;
                side = info.Side ?? DiffSide.Right;
                file = await info.Session.GetFile(relativePath);
                session = info.Session;
                await UpdateThread();
            }
            else
            {
                relativePath = sessionManager.GetRelativePath(buffer);
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
                log.Error("Error updating InlineCommentViewModel", e);
            }
        }

        async Task UpdateThread()
        {
            var placeholderBody = await GetPlaceholderBodyToPreserve();

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

            if (thread != null)
            {
                Thread = new InlineCommentThreadViewModel(session, thread.Comments);
            }
            else
            {
                var newThread = new NewInlineCommentThreadViewModel(session, file, lineNumber, leftBuffer);
                Thread = newThread;
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

        async Task<string> GetPlaceholderBodyToPreserve()
        {
            var lastComment = Thread?.Comments.LastOrDefault();

            if (lastComment?.EditState == CommentEditState.Editing)
            {
                var executing = await lastComment.CommitEdit.IsExecuting.FirstAsync();
                if (!executing) return lastComment.Body;
            }

            return null;
        }
    }
}
