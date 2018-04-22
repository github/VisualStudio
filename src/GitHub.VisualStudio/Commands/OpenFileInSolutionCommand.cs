using System;
using System.ComponentModel.Composition;
using GitHub.Services;
using GitHub.VisualStudio;
using GitHub.Services.Vssdk.Commands;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Differencing;
using Task = System.Threading.Tasks.Task;

namespace GitHub.Commands
{
    [Export(typeof(IOpenFileInSolutionCommand))]
    public class OpenFileInSolutionCommand : VsCommand, IOpenFileInSolutionCommand
    {
        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = Guids.guidContextMenuSet;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = PkgCmdIDList.openFileInSolutionCommand;

        readonly IGitHubServiceProvider serviceProvider;
        readonly Lazy<IVsTextManager> textManager;
        readonly Lazy<IVsEditorAdaptersFactoryService> editorAdapter;
        readonly Lazy<IPullRequestSessionManager> sessionManager;
        readonly Lazy<IPullRequestEditorService> pullRequestEditorService;
        readonly Lazy<IStatusBarNotificationService> statusBar;
        readonly Lazy<IUsageTracker> usageTracker;

        [ImportingConstructor]
        public OpenFileInSolutionCommand(
            IGitHubServiceProvider serviceProvider,
            Lazy<IVsEditorAdaptersFactoryService> editorAdapter,
            Lazy<IPullRequestSessionManager> sessionManager,
            Lazy<IPullRequestEditorService> pullRequestEditorService,
            Lazy<IStatusBarNotificationService> statusBar,
            Lazy<IUsageTracker> usageTracker) : base(CommandSet, CommandId)
        {
            textManager = new Lazy<IVsTextManager>(() => serviceProvider.GetService<SVsTextManager, IVsTextManager>());
            this.serviceProvider = serviceProvider;
            this.editorAdapter = editorAdapter;
            this.sessionManager = sessionManager;
            this.pullRequestEditorService = pullRequestEditorService;
            this.statusBar = statusBar;
            this.usageTracker = usageTracker;
        }

        public override async Task Execute()
        {
            try
            {
                var sourceView = pullRequestEditorService.Value.FindActiveView();
                if (sourceView == null)
                {
                    ShowErrorInStatusBar("Couldn't find active source view");
                    return;
                }

                var textView = editorAdapter.Value.GetWpfTextView(sourceView);

                var info = sessionManager.Value.GetTextBufferInfo(textView.TextBuffer);
                if (info != null)
                {
                    if (!info.Session.IsCheckedOut)
                    {
                        ShowInfoMessage(App.Resources.NavigateToEditorNotCheckedOutInfoMessage);
                        return;
                    }

                    // Navigate from PR file to solution
                    await usageTracker.Value.IncrementCounter(x => x.NumberOfPRDetailsNavigateToEditor);

                    var fileTextView = await pullRequestEditorService.Value.OpenFile(info.Session, info.RelativePath, true);
                    if (fileTextView == null)
                    {
                        ShowErrorInStatusBar("Couldn't find active target view");
                        return;
                    }

                    var fileView = editorAdapter.Value.GetViewAdapter(fileTextView);
                    pullRequestEditorService.Value.NavigateToEquivalentPosition(sourceView, fileView);
                    return;
                }

                var relativePath = sessionManager.Value.GetRelativePath(textView.TextBuffer);
                if (relativePath == null)
                {
                    ShowErrorInStatusBar("File isn't part of repository");
                    return;
                }

                var session = sessionManager.Value.CurrentSession;
                if (session == null)
                {
                    ShowErrorInStatusBar("Couldn't find Pull request session");
                    return;
                }

                // Navigate from working file to PR diff
                // TODO: add metrics
                // await usageTracker.Value.IncrementCounter(x => x.NumberOf???);

                var diffViewer = await pullRequestEditorService.Value.OpenDiff(session, relativePath, "HEAD", scrollToFirstDiff: false);
                if (diffViewer == null)
                {
                    ShowErrorInStatusBar("Couldn't find active diff viewer");
                    return;
                }

                var diffTextView = FindActiveTextView(diffViewer);
                var diffView = editorAdapter.Value.GetViewAdapter(diffTextView);
                pullRequestEditorService.Value.NavigateToEquivalentPosition(sourceView, diffView);
            }
            catch (Exception e)
            {
                ShowErrorInStatusBar("Error navigating to editor", e);
            }
        }

        ITextView FindActiveTextView(IDifferenceViewer diffViewer)
        {
            switch (diffViewer.ActiveViewType)
            {
                case DifferenceViewType.InlineView:
                    return diffViewer.InlineView;
                case DifferenceViewType.LeftView:
                    return diffViewer.LeftView;
                case DifferenceViewType.RightView:
                    return diffViewer.RightView;
            }

            return null;
        }

        void ShowInfoMessage(string message)
        {
            ErrorHandler.ThrowOnFailure(VsShellUtilities.ShowMessageBox(
                serviceProvider, message, null,
                OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST));
        }

        void ShowErrorInStatusBar(string message)
        {
            statusBar.Value.ShowMessage(message);
        }

        void ShowErrorInStatusBar(string message, Exception e)
        {
            statusBar.Value.ShowMessage(message + ": " + e.Message);
        }
    }
}
