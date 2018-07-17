using System;
using System.IO;
using System.ComponentModel.Composition;
using GitHub.Services;
using GitHub.Extensions;
using GitHub.VisualStudio;
using GitHub.Services.Vssdk.Commands;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Differencing;
using Microsoft.VisualStudio.TextManager.Interop;
using Task = System.Threading.Tasks.Task;

namespace GitHub.Commands
{
    /// <summary>
    /// Navigate from a PR file to the equivalent file and location in the editor (or the reverse).
    /// </summary>
    /// <remarks>
    /// This command will do one of the following depending on context.
    /// Navigate from PR file diff to the working file in the solution.
    /// Navigate from the working file in the solution to the PR file diff.
    /// Navigate from an editable diff (e.g. 'View Changes in Solution') to the editor view.
    /// </remarks>
    [Export(typeof(IGoToSolutionOrPullRequestFileCommand))]
    public class GoToSolutionOrPullRequestFileCommand : VsCommand, IGoToSolutionOrPullRequestFileCommand
    {
        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = Guids.guidContextMenuSet;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = PkgCmdIDList.goToSolutionOrPullRequestFileCommand;

        readonly IGitHubServiceProvider serviceProvider;
        readonly Lazy<IVsEditorAdaptersFactoryService> editorAdapter;
        readonly Lazy<IPullRequestSessionManager> sessionManager;
        readonly Lazy<IPullRequestEditorService> pullRequestEditorService;
        readonly Lazy<ITeamExplorerContext> teamExplorerContext;
        readonly Lazy<IGitHubContextService> gitHubContextService;
        readonly Lazy<IStatusBarNotificationService> statusBar;
        readonly Lazy<IUsageTracker> usageTracker;

        [ImportingConstructor]
        public GoToSolutionOrPullRequestFileCommand(
            IGitHubServiceProvider serviceProvider,
            Lazy<IVsEditorAdaptersFactoryService> editorAdapter,
            Lazy<IPullRequestSessionManager> sessionManager,
            Lazy<IPullRequestEditorService> pullRequestEditorService,
            Lazy<ITeamExplorerContext> teamExplorerContext,
            Lazy<IGitHubContextService> gitHubContextService,
            Lazy<IStatusBarNotificationService> statusBar,
            Lazy<IUsageTracker> usageTracker) : base(CommandSet, CommandId)
        {
            this.serviceProvider = serviceProvider;
            this.editorAdapter = editorAdapter;
            this.sessionManager = sessionManager;
            this.pullRequestEditorService = pullRequestEditorService;
            this.gitHubContextService = gitHubContextService;
            this.teamExplorerContext = teamExplorerContext;
            this.statusBar = statusBar;
            this.usageTracker = usageTracker;

            BeforeQueryStatus += OnBeforeQueryStatus;
        }

        public override async Task Execute()
        {
            usageTracker.Value.IncrementCounter(x => x.ExecuteGoToSolutionOrPullRequestFileCommand).Forget();

            try
            {
                var sourceView = pullRequestEditorService.Value.FindActiveView();
                if (sourceView == null)
                {
                    ShowErrorInStatusBar("Couldn't find active source view");
                    return;
                }

                var textView = editorAdapter.Value.GetWpfTextView(sourceView);

                bool isEditableDiff = pullRequestEditorService.Value.IsEditableDiff(textView);
                if (isEditableDiff)
                {
                    // Navigating from editable diff to code view
                    await usageTracker.Value.IncrementCounter(x => x.NumberOfNavigateToCodeView);

                    // Open active document in Code View
                    pullRequestEditorService.Value.OpenActiveDocumentInCodeView(sourceView);
                    return;
                }

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

                if (TryNavigateFromHistoryFile(sourceView))
                {
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

                // Navigate from solution file to PR diff file
                await usageTracker.Value.IncrementCounter(x => x.NumberOfNavigateToPullRequestFileDiff);

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
                ShowErrorInStatusBar("Error Navigating", e);
            }
        }

        void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            try
            {
                var sourceView = pullRequestEditorService.Value.FindActiveView();
                if (sourceView == null)
                {
                    // No active text view
                    Visible = false;
                    return;
                }

                var textView = editorAdapter.Value.GetWpfTextView(sourceView);

                if (pullRequestEditorService.Value.IsEditableDiff(textView))
                {
                    // Active text view is editable diff
                    Text = "Open File in Code View";
                    Visible = true;
                    return;
                }

                var info = sessionManager.Value.GetTextBufferInfo(textView.TextBuffer);
                if (info != null)
                {
                    // Active text view is a PR file
                    Text = "Open File in Solution";
                    Visible = true;
                    return;
                }

                var session = sessionManager.Value.CurrentSession;
                if (session != null)
                {
                    var relativePath = sessionManager.Value.GetRelativePath(textView.TextBuffer);
                    if (relativePath != null)
                    {
                        // Active text view is part of a repository
                        Text = "View Changes in #" + session.PullRequest.Number;
                        Visible = true;
                        return;
                    }
                }

                if (TryNavigateFromHistoryFileQueryStatus(sourceView))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                ShowErrorInStatusBar("Error QueryStatus", ex);
            }

            Visible = false;
        }

        bool TryNavigateFromHistoryFileQueryStatus(IVsTextView sourceView)
        {
            if (teamExplorerContext.Value.ActiveRepository?.LocalPath == null)
            {
                // Only available when there's an active repository
                return false;
            }

            var filePath = FindPath(sourceView);
            if (filePath == null)
            {
                return false;
            }

            var objectish = gitHubContextService.Value.FindObjectishForTFSTempFile(filePath);
            if (objectish == null)
            {
                // Not a temporary Team Explorer blob file
                return false;
            }

            // Navigate from history file is active
            Text = "Open File in Solution";
            Visible = true;
            return true;
        }

        bool TryNavigateFromHistoryFile(IVsTextView sourceView)
        {
            var repositoryDir = teamExplorerContext.Value.ActiveRepository?.LocalPath;
            if (repositoryDir == null)
            {
                return false;
            }

            var path = FindPath(sourceView);
            if (path == null)
            {
                return false;
            }

            var objectish = gitHubContextService.Value.FindObjectishForTFSTempFile(path);
            if (objectish == null)
            {
                return false;
            }

            var (commitSha, blobPath) = gitHubContextService.Value.ResolveBlobFromHistory(repositoryDir, objectish);
            if (blobPath == null)
            {
                return false;
            }

            var workingFile = Path.Combine(repositoryDir, blobPath);
            VsShellUtilities.OpenDocument(serviceProvider, workingFile, VSConstants.LOGVIEWID.TextView_guid,
                out IVsUIHierarchy hierarchy, out uint itemID, out IVsWindowFrame windowFrame, out IVsTextView targetView);

            pullRequestEditorService.Value.NavigateToEquivalentPosition(sourceView, targetView);
            return true;
        }

        // See http://microsoft.public.vstudio.extensibility.narkive.com/agfoD1GO/full-pathname-of-file-shown-in-current-view-of-core-editor#post2
        static string FindPath(IVsTextView textView)
        {
            ErrorHandler.ThrowOnFailure(textView.GetBuffer(out IVsTextLines buffer));
            var userData = buffer as IVsUserData;
            if (userData == null)
            {
                return null;
            }

            ErrorHandler.ThrowOnFailure(userData.GetData(typeof(IVsUserData).GUID, out object data));
            return data as string;
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
