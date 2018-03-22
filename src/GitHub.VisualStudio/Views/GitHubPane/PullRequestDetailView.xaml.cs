using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using GitHub.Settings;
using GitHub.Commands;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using GitHub.UI.Helpers;
using GitHub.ViewModels.GitHubPane;
using GitHub.VisualStudio.UI.Helpers;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.TextManager.Interop;
using ReactiveUI;
using Task = System.Threading.Tasks.Task;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    public class GenericPullRequestDetailView : ViewBase<IPullRequestDetailViewModel, GenericPullRequestDetailView>
    { }

    [ExportViewFor(typeof(IPullRequestDetailViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestDetailView : GenericPullRequestDetailView
    {
        public PullRequestDetailView()
        {
            InitializeComponent();

            bodyMarkdown.PreviewMouseWheel += ScrollViewerUtilities.FixMouseWheelScroll;
            changesSection.PreviewMouseWheel += ScrollViewerUtilities.FixMouseWheelScroll;

            this.WhenActivated(d =>
            {
                d(ViewModel.OpenOnGitHub.Subscribe(_ => DoOpenOnGitHub()));
                d(ViewModel.DiffFile.Subscribe(x => DoDiffFile((IPullRequestFileNode)x, false).Forget()));
                d(ViewModel.ViewFile.Subscribe(x => DoOpenFile((IPullRequestFileNode)x, false).Forget()));
                d(ViewModel.DiffFileWithWorkingDirectory.Subscribe(x => DoDiffFile((IPullRequestFileNode)x, true).Forget()));
                d(ViewModel.OpenFileInWorkingDirectory.Subscribe(x => DoOpenFile((IPullRequestFileNode)x, true).Forget()));
            });

            bodyGrid.RequestBringIntoView += BodyFocusHack;
        }

        [Import]
        ITeamExplorerServiceHolder TeamExplorerServiceHolder { get; set; }

        [Import]
        IVisualStudioBrowser VisualStudioBrowser { get; set; }

        [Import]
        IEditorOptionsFactoryService EditorOptionsFactoryService { get; set; }

        [Import]
        IUsageTracker UsageTracker { get; set; }

        [Import]
        IPullRequestEditorService NavigationService { get; set; }

        [Import]
        IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        [Import]
        IPackageSettings PackageSettings { get; set; }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
        }

        void DoOpenOnGitHub()
        {
            var browser = VisualStudioBrowser;
            var cloneUrl = ViewModel.LocalRepository.CloneUrl;
            var url = ToPullRequestUrl(cloneUrl.Host, ViewModel.RemoteRepositoryOwner, ViewModel.LocalRepository.Name, ViewModel.Model.Number);
            browser.OpenUrl(url);
        }

        static Uri ToPullRequestUrl(string host, string owner, string repositoryName, int number)
        {
            var url = string.Format(CultureInfo.InvariantCulture, "https://{0}/{1}/{2}/pull/{3}", host, owner, repositoryName, number);
            return new Uri(url);
        }

        async Task DoOpenFile(IPullRequestFileNode file, bool workingDirectory)
        {
            try
            {
                var fullPath = ViewModel.GetLocalFilePath(file);
                var fileName = workingDirectory ? fullPath : await ViewModel.ExtractFile(file, true);

                using (workingDirectory ? null : OpenInProvisionalTab())
                using (workingDirectory ? EnableEditorComments() : null)
                {
                    var window = Services.Dte.ItemOperations.OpenFile(fileName);
                    window.Document.ReadOnly = !workingDirectory;

                    if (!workingDirectory)
                    {
                        var buffer = GetBufferAt(fileName);
                        AddBufferTag(buffer, ViewModel.Session, fullPath, null);

                        var textView = NavigationService.FindActiveView();
                        EnableNavigateToEditor(textView, file);
                    }
                }

                if (workingDirectory)
                    await UsageTracker.IncrementCounter(x => x.NumberOfPRDetailsOpenFileInSolution);
                else
                    await UsageTracker.IncrementCounter(x => x.NumberOfPRDetailsViewFile);
            }
            catch (Exception e)
            {
                ShowErrorInStatusBar("Error opening file", e);
            }
        }

        async Task DoNavigateToEditor(IPullRequestFileNode file)
        {
            try
            {
                if (!ViewModel.IsCheckedOut)
                {
                    ShowInfoMessage("Checkout PR branch before opening file in solution.");
                    return;
                }

                var fullPath = ViewModel.GetLocalFilePath(file);

                var activeView = NavigationService.FindActiveView();
                if (activeView == null)
                {
                    ShowErrorInStatusBar("Couldn't find active view");
                    return;
                }

                using (EnableEditorComments())
                {
                    NavigationService.NavigateToEquivalentPosition(activeView, fullPath);
                }

                await UsageTracker.IncrementCounter(x => x.NumberOfPRDetailsNavigateToEditor);
            }
            catch (Exception e)
            {
                ShowErrorInStatusBar("Error navigating to editor", e);
            }
        }

        static void ShowInfoMessage(string message)
        {
            ErrorHandler.ThrowOnFailure(VsShellUtilities.ShowMessageBox(
                Services.GitHubServiceProvider, message, null,
                OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST));
        }

        async Task DoDiffFile(IPullRequestFileNode file, bool workingDirectory)
        {
            try
            {
                var rightPath = System.IO.Path.Combine(file.DirectoryPath, file.FileName);
                var leftPath = file.OldPath ?? rightPath;
                var rightFile = workingDirectory ? ViewModel.GetLocalFilePath(file) : await ViewModel.ExtractFile(file, true);
                var leftFile = await ViewModel.ExtractFile(file, false);
                var leftLabel = $"{leftPath};{ViewModel.TargetBranchDisplayName}";
                var rightLabel = workingDirectory ? rightPath : $"{rightPath};PR {ViewModel.Model.Number}";
                var caption = $"Diff - {file.FileName}";
                var options = __VSDIFFSERVICEOPTIONS.VSDIFFOPT_DetectBinaryFiles |
                    __VSDIFFSERVICEOPTIONS.VSDIFFOPT_LeftFileIsTemporary;

                if (!workingDirectory)
                {
                    options |= __VSDIFFSERVICEOPTIONS.VSDIFFOPT_RightFileIsTemporary;
                }

                IVsWindowFrame frame;
                using (OpenInProvisionalTab())
                {
                    var tooltip = $"{leftLabel}\nvs.\n{rightLabel}";

                    // Diff window will open in provisional (right hand) tab until document is touched.
                    frame = GitHub.VisualStudio.Services.DifferenceService.OpenComparisonWindow2(
                        leftFile,
                        rightFile,
                        caption,
                        tooltip,
                        leftLabel,
                        rightLabel,
                        string.Empty,
                        string.Empty,
                        (uint)options);
                }

                object docView;
                frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView);
                var diffViewer = ((IVsDifferenceCodeWindow)docView).DifferenceViewer;

                var session = ViewModel.Session;
                AddBufferTag(diffViewer.LeftView.TextBuffer, session, leftPath, DiffSide.Left);

                if (!workingDirectory)
                {
                    AddBufferTag(diffViewer.RightView.TextBuffer, session, rightPath, DiffSide.Right);
                    EnableNavigateToEditor(diffViewer.LeftView, file);
                    EnableNavigateToEditor(diffViewer.RightView, file);
                    EnableNavigateToEditor(diffViewer.InlineView, file);
                }

                if (workingDirectory)
                    await UsageTracker.IncrementCounter(x => x.NumberOfPRDetailsCompareWithSolution);
                else
                    await UsageTracker.IncrementCounter(x => x.NumberOfPRDetailsViewChanges);
            }
            catch (Exception e)
            {
                ShowErrorInStatusBar("Error opening file", e);
            }
        }

        void AddBufferTag(ITextBuffer buffer, IPullRequestSession session, string path, DiffSide? side)
        {
            buffer.Properties.GetOrCreateSingletonProperty(
                typeof(PullRequestTextBufferInfo),
                () => new PullRequestTextBufferInfo(session, path, side));

            var projection = buffer as IProjectionBuffer;

            if (projection != null)
            {
                foreach (var source in projection.SourceBuffers)
                {
                    AddBufferTag(source, session, path, side);
                }
            }
        }

        void EnableNavigateToEditor(IWpfTextView textView, IPullRequestFileNode file)
        {
            var view = EditorAdaptersFactoryService.GetViewAdapter(textView);
            EnableNavigateToEditor(view, file);
        }

        void EnableNavigateToEditor(IVsTextView textView, IPullRequestFileNode file)
        {
            var commandGroup = VSConstants.CMDSETID.StandardCommandSet2K_guid;
            var commandId = (int)VSConstants.VSStd2KCmdID.RETURN;
            new TextViewCommandDispatcher(textView, commandGroup, commandId).Exec += async (s, e) => await DoNavigateToEditor(file);

            var contextMenuCommandGroup = new Guid(Guids.guidContextMenuSetString);
            var goToCommandId = PkgCmdIDList.openFileInSolutionCommand;
            new TextViewCommandDispatcher(textView, contextMenuCommandGroup, goToCommandId).Exec += async (s, e) => await DoNavigateToEditor(file);
        }

        void ShowErrorInStatusBar(string message, Exception e = null)
        {
            var ns = GitHub.VisualStudio.Services.DefaultExportProvider.GetExportedValue<IStatusBarNotificationService>();
            if (e != null)
            {
                message += ": " + e.Message;
            }
            ns?.ShowMessage(message);
        }

        private void FileListKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var file = (e.OriginalSource as FrameworkElement)?.DataContext as IPullRequestFileNode;
                if (file != null)
                {
                    DoDiffFile(file, false).Forget();
                }
            }
        }

        void FileListMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var file = (e.OriginalSource as FrameworkElement)?.DataContext as IPullRequestFileNode;

            if (file != null)
            {
                DoDiffFile(file, false).Forget();
            }
        }

        void FileListMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (e.OriginalSource as Visual)?.GetSelfAndVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();

            if (item != null)
            {
                // Select tree view item on right click.
                item.IsSelected = true;
            }
        }

        ITextBuffer GetBufferAt(string filePath)
        {
            var editorAdapterFactoryService = GitHub.VisualStudio.Services.ComponentModel.GetService<IVsEditorAdaptersFactoryService>();
            IVsUIHierarchy uiHierarchy;
            uint itemID;
            IVsWindowFrame windowFrame;

            if (VsShellUtilities.IsDocumentOpen(
                GitHub.VisualStudio.Services.GitHubServiceProvider,
                filePath,
                Guid.Empty,
                out uiHierarchy,
                out itemID,
                out windowFrame))
            {
                IVsTextView view = VsShellUtilities.GetTextView(windowFrame);
                IVsTextLines lines;
                if (view.GetBuffer(out lines) == 0)
                {
                    var buffer = lines as IVsTextBuffer;
                    if (buffer != null)
                        return editorAdapterFactoryService.GetDataBuffer(buffer);
                }
            }

            return null;
        }

        void TreeView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ApplyContextMenuBinding<TreeViewItem>(sender, e);
        }

        void ApplyContextMenuBinding<TItem>(object sender, ContextMenuEventArgs e) where TItem : Control
        {
            var container = (Control)sender;
            var item = (e.OriginalSource as Visual)?.GetSelfAndVisualAncestors().OfType<TItem>().FirstOrDefault();

            e.Handled = true;

            if (item != null)
            {
                var fileNode = item.DataContext as IPullRequestFileNode;

                if (fileNode != null)
                {
                    container.ContextMenu.DataContext = this.DataContext;

                    foreach (var menuItem in container.ContextMenu.Items.OfType<MenuItem>())
                    {
                        menuItem.CommandParameter = fileNode;
                    }

                    e.Handled = false;
                }
            }
        }

        void BodyFocusHack(object sender, RequestBringIntoViewEventArgs e)
        {
            if (e.TargetObject == bodyMarkdown)
            {
                // Hack to prevent pane scrolling to top. Instead focus selected tree view item.
                // See https://github.com/github/VisualStudio/issues/1042
                var node = changesTree.GetTreeViewItem(changesTree.SelectedItem);
                node?.Focus();
                e.Handled = true;
            }
        }

        async void ViewFileCommentsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var file = (e.OriginalSource as Hyperlink)?.DataContext as IPullRequestFileNode;

                if (file != null)
                {
                    var param = (object)new InlineCommentNavigationParams
                    {
                        FromLine = -1,
                    };

                    await DoDiffFile(file, false);

                    // HACK: We need to wait here for the diff view to set itself up and move its cursor
                    // to the first changed line. There must be a better way of doing this.
                    await Task.Delay(1500);

                    GitHub.VisualStudio.Services.Dte.Commands.Raise(
                        Guids.CommandSetString,
                        PkgCmdIDList.NextInlineCommentId,
                        ref param,
                        null);
                }
            }
            catch { }
        }

        void OpenHyperlink(object sender, ExecutedRoutedEventArgs e)
        {
            Uri uri;

            if (Uri.TryCreate(e.Parameter?.ToString(), UriKind.Absolute, out uri))
            {
                VisualStudioBrowser.OpenUrl(uri);
            }
        }

        static IDisposable OpenInProvisionalTab()
        {
            return new NewDocumentStateScope
                (__VSNEWDOCUMENTSTATE.NDS_Provisional,
                VSConstants.NewDocumentStateReason.SolutionExplorer);
        }

        IDisposable EnableEditorComments() => new EnableEditorCommentsContext(PackageSettings);

        // Editor comments will be enabled when a file is opened from inside this context.
        class EnableEditorCommentsContext : IDisposable
        {
            readonly IPackageSettings settings;
            bool editorComments;

            internal EnableEditorCommentsContext(IPackageSettings settings)
            {
                this.settings = settings;

                editorComments = settings.EditorComments;
                settings.EditorComments = true;
            }

            public void Dispose()
            {
                settings.EditorComments = editorComments;
            }
        }
    }
}
