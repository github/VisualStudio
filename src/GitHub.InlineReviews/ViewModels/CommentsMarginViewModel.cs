using System.Windows.Input;
using GitHub.Services;
using GitHub.Extensions;
using Microsoft.VisualStudio.Text;
using ReactiveUI;
using Task = System.Threading.Tasks.Task;

namespace GitHub.InlineReviews.ViewModels
{
    public class CommentsMarginViewModel : ReactiveObject
    {
        readonly IPullRequestSessionManager sessionManager;
        readonly ITextBuffer textBuffer;

        int commentsInFile;
        bool marginEnabled;

        public CommentsMarginViewModel(IPullRequestSessionManager sessionManager, ITextBuffer textBuffer, ICommand enableInlineComments)
        {
            this.sessionManager = sessionManager;
            this.textBuffer = textBuffer;

            EnableInlineComments = enableInlineComments;
            InitializeAsync().Forget();
        }

        async Task InitializeAsync()
        {
            await sessionManager.EnsureInitialized();
            var relativePath = sessionManager.GetRelativePath(textBuffer);
            if (relativePath != null)
            {
                var sessionFile = await sessionManager.CurrentSession.GetFile(relativePath);
                CommentsInFile = sessionFile?.InlineCommentThreads?.Count ?? -1;
            }
        }

        public int CommentsInFile
        {
            get { return commentsInFile; }
            private set { this.RaiseAndSetIfChanged(ref commentsInFile, value); }
        }

        public bool MarginEnabled
        {
            get { return marginEnabled; }
            set { this.RaiseAndSetIfChanged(ref marginEnabled, value); }
        }

        public ICommand EnableInlineComments { get; }
    }
}
