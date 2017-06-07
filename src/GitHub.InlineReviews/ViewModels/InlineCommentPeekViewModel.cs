using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Factories;
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
    class InlineCommentPeekViewModel : ReactiveObject
    {
        readonly IApiClientFactory apiClientFactory;
        readonly IPeekSession peekSession;
        readonly IPullRequestSessionManager sessionManager;
        IPullRequestSession session;
        InlineCommentThreadViewModel thread;
        string relativePath;
        bool leftBuffer;
        int? lineNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineCommentPeekViewModel"/> class.
        /// </summary>
        public InlineCommentPeekViewModel(
            IApiClientFactory apiClientFactory,
            IPeekSession peekSession,
            IPullRequestSessionManager sessionManager)
        {
            this.apiClientFactory = apiClientFactory;
            this.peekSession = peekSession;
            this.sessionManager = sessionManager;
        }

        /// <summary>
        /// Gets the thread of comments to display.
        /// </summary>
        public InlineCommentThreadViewModel Thread
        {
            get { return thread; }
            private set { this.RaiseAndSetIfChanged(ref thread, value); }
        }

        public async Task Initialize()
        {
            var info = sessionManager.GetTextBufferInfo(peekSession.TextView.TextBuffer);

            if (info != null)
            {
                relativePath = info.RelativePath;
                leftBuffer = info.IsLeftComparisonBuffer;
                await SessionChanged(info.Session);
            }
            else
            {
                await SessionChanged(sessionManager.CurrentSession);
                sessionManager.WhenAnyValue(x => x.CurrentSession)
                    .Skip(1)
                    .Subscribe(x => SessionChanged(x).Forget());
            }
        }

        async Task LineNumberChanged()
        {
            Thread = null;

            var file = await session.GetFile(relativePath);
            var buffer = peekSession.TextView.TextBuffer;
            lineNumber = peekSession.GetTriggerPoint(buffer.CurrentSnapshot)?.GetContainingLine().LineNumber;

            if (file == null || lineNumber == null)
                return;

            var thread = file.InlineCommentThreads.FirstOrDefault(x => x.LineNumber == lineNumber);

            if (thread == null)
                return;

            Thread = new InlineCommentThreadViewModel(
                CreateApiClient(session.Repository),
                session,
                thread.OriginalCommitSha,
                relativePath,
                thread.OriginalPosition);

            foreach (var comment in thread.Comments)
            {
                Thread.Comments.Add(new InlineCommentViewModel(Thread, session.User, comment));
            }

            Thread.AddReplyPlaceholder();
        }

        async Task SessionChanged(IPullRequestSession session)
        {
            this.session = session;

            if (session == null)
            {
                Thread = null;
                return;
            }

            if (relativePath == null)
            {
                var buffer = peekSession.TextView.TextBuffer;
                var document = buffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument));
                relativePath = session.GetRelativePath(document.FilePath);

                if (relativePath == null)
                    return;
            }

            await LineNumberChanged();
        }

        IApiClient CreateApiClient(ILocalRepositoryModel repository)
        {
            var hostAddress = HostAddress.Create(repository.CloneUrl.Host);
            return apiClientFactory.Create(hostAddress);
        }
    }
}
