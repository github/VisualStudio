using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Services;
using GitHub.UI;
using GitHub.ViewModels;
using GitHub.VisualStudio.Helpers;
using GitHub.VisualStudio.UI.Helpers;
using Microsoft.VisualStudio.Shell.Interop;
using ReactiveUI;

namespace GitHub.VisualStudio.UI.Views
{
    public class GenericPullRequestDetailView : BusyStateView<IPullRequestDetailViewModel, GenericPullRequestDetailView>
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
                d(ViewModel.OpenFile.Subscribe(x => DoOpenFile((IPullRequestFileNode)x).Forget()));
                d(ViewModel.DiffFile.Subscribe(x => DoDiffFile((IPullRequestFileNode)x).Forget()));
            });
        }

        [Import]
        ITeamExplorerServiceHolder TeamExplorerServiceHolder { get; set; }

        [Import]
        IVisualStudioBrowser VisualStudioBrowser { get; set; }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
        }

        void DoOpenOnGitHub()
        {
            var repo = TeamExplorerServiceHolder.ActiveRepo;
            var browser = VisualStudioBrowser;
            var url = repo.CloneUrl.ToRepositoryUrl().Append("pull/" + ViewModel.Model.Number);
            browser.OpenUrl(url);
        }

        async Task DoOpenFile(IPullRequestFileNode file)
        {
            try
            {
                var fileName = await ViewModel.ExtractFile(file);
                var window = Services.Dte.ItemOperations.OpenFile(fileName);

                // If the file we extracted isn't the current file on disk, make the window read-only.
                window.Document.ReadOnly = fileName != file.DirectoryPath;
            }
            catch (Exception e)
            {
                ShowErrorInStatusBar("Error opening file", e);
            }
        }

        async Task DoDiffFile(IPullRequestFileNode file)
        {
            try
            {
                var fileNames = await ViewModel.ExtractDiffFiles(file);
            var leftLabel = $"{file.FileName};{ViewModel.TargetBranchDisplayName}";
            var rightLabel = $"{file.FileName};PR {ViewModel.Model.Number}";

            Services.DifferenceService.OpenComparisonWindow2(
                fileNames.Item1,
                fileNames.Item2,
                $"{leftLabel} vs {rightLabel}",
                file.DirectoryPath,
                leftLabel,
                rightLabel,
                string.Empty,
                string.Empty,
                (int)(__VSDIFFSERVICEOPTIONS.VSDIFFOPT_DetectBinaryFiles |
                    __VSDIFFSERVICEOPTIONS.VSDIFFOPT_LeftFileIsTemporary |
                    __VSDIFFSERVICEOPTIONS.VSDIFFOPT_RightFileIsTemporary));
            }
            catch (Exception e)
            {
                ShowErrorInStatusBar("Error opening file", e);
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
                DoDiffFile(file).Forget();
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
    }
}
