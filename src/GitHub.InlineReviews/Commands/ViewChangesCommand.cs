using System;
using System.ComponentModel.Composition;
using GitHub.Services;
using GitHub.VisualStudio;
using GitHub.Services.Vssdk.Commands;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Task = System.Threading.Tasks.Task;

namespace GitHub.InlineReviews.Commands
{
    [Export(typeof(IViewChangesCommand))]
    public class ViewChangesCommand : VsCommand, IViewChangesCommand
    {
        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = Guids.CommandSetGuid;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = PkgCmdIDList.ViewChangesId;

        readonly Lazy<IVsTextManager> textManager;
        readonly Lazy<IVsEditorAdaptersFactoryService> editorAdapter;
        readonly Lazy<IPullRequestSessionManager> sessionManager;
        readonly Lazy<IPullRequestEditorService> pullRequestEditorService;

        [ImportingConstructor]
        public ViewChangesCommand(
            IGitHubServiceProvider serviceProvider,
            Lazy<IVsEditorAdaptersFactoryService> editorAdapter,
            Lazy<IPullRequestSessionManager> sessionManager,
            Lazy<IPullRequestEditorService> pullRequestEditorService) : base(CommandSet, CommandId)
        {
            textManager = new Lazy<IVsTextManager>(() => serviceProvider.GetService<SVsTextManager, IVsTextManager>());
            this.editorAdapter = editorAdapter;
            this.sessionManager = sessionManager;
            this.pullRequestEditorService = pullRequestEditorService;
        }

        public override async Task Execute()
        {
            var session = sessionManager.Value.CurrentSession;
            if (session == null)
            {
                // No current PR session
                return;
            }

            IVsTextView activeView = null;
            if (textManager.Value.GetActiveView(1, null, out activeView) != VSConstants.S_OK)
            {
                // No active file
                return;
            }

            var textView = editorAdapter.Value.GetWpfTextView(activeView);
            var relativePath = sessionManager.Value.GetRelativePath(textView.TextBuffer);
            if (relativePath == null)
            {
                // File not part of PR
                return;
            }

            await pullRequestEditorService.Value.OpenDiff(session, relativePath, "HEAD");
        }
    }
}
