using System;
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
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// Represents the contents of an inline comment peek view displayed in an editor.
    /// </summary>
    public sealed class InlineCommentPeekViewModel : ReactiveObject, IDisposable
    {
        readonly IApiClientFactory apiClientFactory;
        readonly IInlineCommentPeekService peekService;
        readonly IPeekSession peekSession;
        readonly IPullRequestSessionManager sessionManager;
        IPullRequestSession session;
        IPullRequestSessionFile file;
        IDisposable fileSubscription;
        ICommentThreadViewModel thread;
        IDisposable threadSubscription;
        ITrackingPoint triggerPoint;
        string fullPath;
        bool leftBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineCommentPeekViewModel"/> class.
        /// </summary>
        public InlineCommentPeekViewModel(
            IApiClientFactory apiClientFactory,
            IInlineCommentPeekService peekService,
            IPeekSession peekSession,
            IPullRequestSessionManager sessionManager,
            INextInlineCommentCommand nextCommentCommand,
            IPreviousInlineCommentCommand previousCommentCommand)
        {
            Guard.ArgumentNotNull(apiClientFactory, nameof(apiClientFactory));
            Guard.ArgumentNotNull(peekService, nameof(peekService));
            Guard.ArgumentNotNull(peekSession, nameof(peekSession));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));
            Guard.ArgumentNotNull(nextCommentCommand, nameof(nextCommentCommand));
            Guard.ArgumentNotNull(previousCommentCommand, nameof(previousCommentCommand));

            this.apiClientFactory = apiClientFactory;
            this.peekService = peekService;
            this.peekSession = peekSession;
            this.sessionManager = sessionManager;
            triggerPoint = peekSession.GetTriggerPoint(peekSession.TextView.TextBuffer);

            peekSession.Dismissed += (s, e) => Dispose();

            NextComment = ReactiveCommand.CreateAsyncTask(_ =>
                nextCommentCommand.Execute(new InlineCommentNavigationParams
                {
                    FromLine = peekService.GetLineNumber(peekSession, triggerPoint).Item1,
                }));

            PreviousComment = ReactiveCommand.CreateAsyncTask(_ =>
                previousCommentCommand.Execute(new InlineCommentNavigationParams
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
            fileSubscription?.Dispose();
            fileSubscription = null;
        }

        public async Task Initialize()
        {
            var buffer = peekSession.TextView.TextBuffer;
            var info = sessionManager.GetTextBufferInfo(buffer);

            if (info != null)
            {
                fullPath = info.FilePath;
                leftBuffer = info.IsLeftComparisonBuffer;
                await SessionChanged(info.Session);
            }
            else
            {
                var document = buffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument));
                fullPath = document.FilePath;

                await SessionChanged(sessionManager.CurrentSession);
                sessionManager.WhenAnyValue(x => x.CurrentSession)
                    .Skip(1)
                    .Subscribe(x => SessionChanged(x).Forget());
            }
        }

        async Task UpdateThread()
        {
            var placeholderBody = GetPlaceholderBody();

            Thread = null;
            threadSubscription?.Dispose();

            if (file == null)
                return;

            var lineAndLeftBuffer = peekService.GetLineNumber(peekSession, triggerPoint);
            var lineNumber = lineAndLeftBuffer.Item1;
            var leftBuffer = lineAndLeftBuffer.Item2;
            var thread = file.InlineCommentThreads.FirstOrDefault(x => 
                x.LineNumber == lineNumber &&
                (!leftBuffer || x.DiffLineType == DiffChangeType.Delete));
            var apiClient = CreateApiClient(session.Repository);

            if (thread != null)
            {
                Thread = new InlineCommentThreadViewModel(apiClient, session, thread.Comments);
            }
            else
            {
                var newThread = new NewInlineCommentThreadViewModel(apiClient, session, file, lineNumber, leftBuffer);
                threadSubscription = newThread.Finished.Subscribe(_ => UpdateThread().Forget());
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

        async Task SessionChanged(IPullRequestSession session)
        {
            this.session = session;
            fileSubscription?.Dispose();

            if (session == null)
            {
                Thread = null;
                threadSubscription?.Dispose();
                return;
            }

            var relativePath = session.GetRelativePath(fullPath);
            file = await session.GetFile(relativePath);
            fileSubscription = file.WhenAnyValue(x => x.InlineCommentThreads).Subscribe(_ => UpdateThread().Forget());
        }

        IApiClient CreateApiClient(ILocalRepositoryModel repository)
        {
            var hostAddress = HostAddress.Create(repository.CloneUrl.Host);
            return apiClientFactory.Create(hostAddress);
        }

        string GetPlaceholderBody()
        {
            var lastComment = Thread?.Comments.LastOrDefault();
            return lastComment?.EditState == CommentEditState.Editing ? lastComment.Body : null;
        }
    }
}
