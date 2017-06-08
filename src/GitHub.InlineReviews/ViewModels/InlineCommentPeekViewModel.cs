using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Factories;
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
    public class InlineCommentPeekViewModel : ReactiveObject
    {
        readonly IApiClientFactory apiClientFactory;
        readonly IInlineCommentPeekService peekService;
        readonly IPeekSession peekSession;
        readonly IPullRequestSessionManager sessionManager;
        IPullRequestSession session;
        IPullRequestSessionFile file;
        ICommentThreadViewModel thread;
        string fullPath;
        bool leftBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineCommentPeekViewModel"/> class.
        /// </summary>
        public InlineCommentPeekViewModel(
            IApiClientFactory apiClientFactory,
            IInlineCommentPeekService peekService,
            IPeekSession peekSession,
            IPullRequestSessionManager sessionManager)
        {
            this.apiClientFactory = apiClientFactory;
            this.peekService = peekService;
            this.peekSession = peekSession;
            this.sessionManager = sessionManager;
        }

        /// <summary>
        /// Gets the thread of comments to display.
        /// </summary>
        public ICommentThreadViewModel Thread
        {
            get { return thread; }
            private set { this.RaiseAndSetIfChanged(ref thread, value); }
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

        void UpdateThread()
        {
            Thread = null;

            if (file == null)
                return;

            var buffer = peekSession.TextView.TextBuffer;
            var lineNumber = peekService.GetLineNumber(peekSession);

            if (lineNumber == null)
                return;

            var thread = file.InlineCommentThreads.FirstOrDefault(x => x.LineNumber == lineNumber);
            var apiClient = CreateApiClient(session.Repository);

            if (thread != null)
            {
                Thread = new InlineCommentThreadViewModel(apiClient, session, thread.Comments);
            }
            else
            {
                Thread = new NewInlineCommentThreadViewModel(apiClient, session, file, lineNumber.Value, leftBuffer);
            }
        }

        async Task SessionChanged(IPullRequestSession session)
        {
            this.session = session;

            if (session == null)
            {
                Thread = null;
                return;
            }

            var relativePath = session.GetRelativePath(fullPath);
            file = await session.GetFile(relativePath);
            UpdateThread();
        }

        IApiClient CreateApiClient(ILocalRepositoryModel repository)
        {
            var hostAddress = HostAddress.Create(repository.CloneUrl.Host);
            return apiClientFactory.Create(hostAddress);
        }
    }
}
