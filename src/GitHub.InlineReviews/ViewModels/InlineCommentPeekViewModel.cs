using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Commands;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Factories;
using GitHub.InlineReviews.Services;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using ReactiveUI;
using Serilog;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// Represents the contents of an inline comment peek view displayed in an editor.
    /// </summary>
    public sealed class InlineCommentPeekViewModel : ReactiveObject, IDisposable
    {
        static readonly ILogger log = LogManager.ForContext<InlineCommentPeekViewModel>();
        readonly IInlineCommentPeekService peekService;
        readonly IPeekSession peekSession;
        readonly IPullRequestSessionManager sessionManager;
        readonly IViewViewModelFactory factory;
        IPullRequestSession session;
        IPullRequestSessionFile file;
        IPullRequestReviewCommentThreadViewModel thread;
        IReadOnlyList<IInlineAnnotationViewModel> annotations;
        IDisposable fileSubscription;
        IDisposable sessionSubscription;
        IDisposable threadSubscription;
        ITrackingPoint triggerPoint;
        string relativePath;
        DiffSide side;
        bool availableForComment;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineCommentPeekViewModel"/> class.
        /// </summary>
        public InlineCommentPeekViewModel(IInlineCommentPeekService peekService,
            IPeekSession peekSession,
            IPullRequestSessionManager sessionManager,
            INextInlineCommentCommand nextCommentCommand,
            IPreviousInlineCommentCommand previousCommentCommand,
            IViewViewModelFactory factory)
        {
            Guard.ArgumentNotNull(peekService, nameof(peekService));
            Guard.ArgumentNotNull(peekSession, nameof(peekSession));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));
            Guard.ArgumentNotNull(nextCommentCommand, nameof(nextCommentCommand));
            Guard.ArgumentNotNull(previousCommentCommand, nameof(previousCommentCommand));
            Guard.ArgumentNotNull(factory, nameof(factory));

            this.peekService = peekService;
            this.peekSession = peekSession;
            this.sessionManager = sessionManager;
            this.factory = factory;
            triggerPoint = peekSession.GetTriggerPoint(peekSession.TextView.TextBuffer);

            peekSession.Dismissed += (s, e) => Dispose();

            Close = this.WhenAnyValue(x => x.Thread)
                .Where(x => x != null)
                .SelectMany(x => x.IsNewThread
                    ? x.Comments.Single().CancelEdit.SelectUnit()
                    : Observable.Never<Unit>());

            NextComment = ReactiveCommand.CreateFromTask(
                () => nextCommentCommand.Execute(new InlineCommentNavigationParams
                {
                    FromLine = peekService.GetLineNumber(peekSession, triggerPoint).Item1,
                }),
                Observable.Return(nextCommentCommand.Enabled));

            PreviousComment = ReactiveCommand.CreateFromTask(
                () => previousCommentCommand.Execute(new InlineCommentNavigationParams
                {
                    FromLine = peekService.GetLineNumber(peekSession, triggerPoint).Item1,
                }),
                Observable.Return(previousCommentCommand.Enabled));
        }

        public bool AvailableForComment
        {
            get { return availableForComment; }
            private set { this.RaiseAndSetIfChanged(ref availableForComment, value); }
        }

        /// <summary>
        /// Gets the annotations displayed.
        /// </summary>
        public IReadOnlyList<IInlineAnnotationViewModel> Annotations
        {
            get { return annotations; }
            private set { this.RaiseAndSetIfChanged(ref annotations, value); }
        }

        /// <summary>
        /// Gets the thread of comments to display.
        /// </summary>
        public IPullRequestReviewCommentThreadViewModel Thread
        {
            get { return thread; }
            private set { this.RaiseAndSetIfChanged(ref thread, value); }
        }

        /// <summary>
        /// Gets a command which moves to the next inline comment in the file.
        /// </summary>
        public ReactiveCommand<Unit, Unit> NextComment { get; }

        /// <summary>
        /// Gets a command which moves to the previous inline comment in the file.
        /// </summary>
        public ReactiveCommand<Unit, Unit> PreviousComment { get; }

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
            fileSubscription = file.LinesChanged.ObserveOn(RxApp.MainThreadScheduler).Subscribe(LinesChanged);
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
            Thread = null;
            threadSubscription?.Dispose();

            Annotations = null;

            if (file == null)
                return;

            var lineAndLeftBuffer = peekService.GetLineNumber(peekSession, triggerPoint);
            var lineNumber = lineAndLeftBuffer.Item1;
            var leftBuffer = lineAndLeftBuffer.Item2;

            AvailableForComment =
                file.Diff.Any(chunk => chunk.Lines
                    .Any(line => line.NewLineNumber - 1 == lineNumber));

            var thread = file.InlineCommentThreads?.FirstOrDefault(x =>
                x.LineNumber == lineNumber &&
                ((leftBuffer && x.DiffLineType == DiffChangeType.Delete) || (!leftBuffer && x.DiffLineType != DiffChangeType.Delete)));

            Annotations = file.InlineAnnotations?.Where(model => model.EndLine - 1 == lineNumber)
                .Select(model => new InlineAnnotationViewModel(model))
                .ToArray();

            var threadModel = factory.CreateViewModel<IPullRequestReviewCommentThreadViewModel>();

            if (thread?.Comments.Count > 0)
            {
                await threadModel.InitializeAsync(session, file, thread, true);
            }
            else
            {
                await threadModel.InitializeNewAsync(session, file, lineNumber, side, true);
            }

            Thread = threadModel;
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
    }
}
