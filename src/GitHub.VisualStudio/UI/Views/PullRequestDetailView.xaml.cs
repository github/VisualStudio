using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.InlineReviews.Commands;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using GitHub.UI.Helpers;
using GitHub.ViewModels;
using GitHub.VisualStudio.UI.Helpers;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using ReactiveUI;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Text;
using Microsoft.VisualStudio.Text.Projection;

namespace GitHub.VisualStudio.UI.Views
{
    public class GenericPullRequestDetailView : ViewBase<IPullRequestDetailViewModel, GenericPullRequestDetailView>
    { }

    [ExportView(ViewType = UIViewType.PRDetail)]
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

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
        }

        void DoOpenOnGitHub()
        {
            var repo = ViewModel.RemoteRepository;
            var browser = VisualStudioBrowser;
            var url = repo.CloneUrl.ToRepositoryUrl().Append("pull/" + ViewModel.Model.Number);
            browser.OpenUrl(url);
        }

        async Task DoOpenFile(IPullRequestFileNode file, bool workingDirectory)
        {
            try
            {
                var fullPath = ViewModel.GetLocalFilePath(file);
                var fileName = workingDirectory ? fullPath : await ViewModel.ExtractFile(file, true);

                using (workingDirectory ? null : OpenInProvisionalTab())
                {
                    var window = Services.Dte.ItemOperations.OpenFile(fileName);
                    window.Document.ReadOnly = !workingDirectory;

                    var buffer = GetBufferAt(fileName);

                    if (!workingDirectory)
                    {
                        AddBufferTag(buffer, ViewModel.Session, fullPath, null);
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

        async Task DoDiffFile(IPullRequestFileNode file, bool workingDirectory)
        {
            try
            {
                var relativePath = System.IO.Path.Combine(file.DirectoryPath, file.FileName);
                var rightFile = workingDirectory ? ViewModel.GetLocalFilePath(file) : await ViewModel.ExtractFile(file, true);
                var leftFile = await ViewModel.ExtractFile(file, false);
                var fullPath = System.IO.Path.Combine(ViewModel.LocalRepository.LocalPath, relativePath);
                var leftLabel = $"{relativePath};{ViewModel.TargetBranchDisplayName}";
                var rightLabel = workingDirectory ? relativePath : $"{relativePath};PR {ViewModel.Model.Number}";
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
                    frame = Services.DifferenceService.OpenComparisonWindow2(
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
                AddBufferTag(diffViewer.LeftView.TextBuffer, session, relativePath, DiffSide.Left);

                if (!workingDirectory)
                {
                    AddBufferTag(diffViewer.RightView.TextBuffer, session, relativePath, DiffSide.Right);
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

        void ShowErrorInStatusBar(string message, Exception e)
        {
            var ns = Services.DefaultExportProvider.GetExportedValue<IStatusBarNotificationService>();
            ns?.ShowMessage(message + ": " + e.Message);
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
            var editorAdapterFactoryService = Services.ComponentModel.GetService<IVsEditorAdaptersFactoryService>();
            IVsUIHierarchy uiHierarchy;
            uint itemID;
            IVsWindowFrame windowFrame;

            if (VsShellUtilities.IsDocumentOpen(
                Services.GitHubServiceProvider,
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

        void ViewCommentsClick(object sender, RoutedEventArgs e)
        {
            var model = (object)ViewModel.Model;
            Services.Dte.Commands.Raise(
                Guids.CommandSetString,
                PkgCmdIDList.ShowPullRequestCommentsId,
                ref model,
                null);
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

                    Services.Dte.Commands.Raise(
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
    }
}
