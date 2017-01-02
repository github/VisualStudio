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

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(_ => InitializeChangesSectionCommands());

            this.WhenActivated(d =>
            {
                d(ViewModel.OpenOnGitHub.Subscribe(_ => DoOpenOnGitHub()));
                d(ViewModel.OpenFile.Subscribe(x => DoOpenFile((IPullRequestFileNode)x).Forget()));
                d(ViewModel.DiffFile.Subscribe(x => DoDiffFile((IPullRequestFileNode)x).Forget()));
            });

            OpenChangesOptionsMenu = ReactiveCommand.Create();
            OpenChangesOptionsMenu.Subscribe(DoOpenChangesOptionsMenu);
        }

        public ReactiveCommand<object> OpenChangesOptionsMenu { get; }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
        }

        void InitializeChangesSectionCommands()
        {
            // As far as I can tell, SectionControl.SectionCommands is only available in Team
            // Foundation 15 and greater - except that it's also available in VS2015 somehow!
            // For the moment use reflection to try to get hold of the property.
            var sectionCommandsProperty = changesSection.GetType().GetProperty("SectionCommands");
            var commandType = sectionCommandsProperty?
                .DeclaringType
                .Assembly
                .GetType("Microsoft.TeamFoundation.Controls.WPF.TeamExplorer.TeamExplorerSectionCommand");
            var commandCtor = commandType?.GetConstructor(new[] 
            {
                typeof(ICommand), typeof(string), typeof(object)
            });

            if (sectionCommandsProperty != null && commandCtor != null)
            {
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(commandType));

                var iconDrawing = new GeometryDrawing
                {
                    Geometry = OcticonPath.GetGeometryForIcon(Octicon.three_bars),
                };

                // I can't find a way to bind a DynamicResource to GeometryDrawing.Path, so bind the brush we
                // want to this.Foreground and bind GeometryDrawing.Path to that.
                var brushBinding = new Binding(nameof(Foreground));
                brushBinding.Source = this;
                BindingOperations.SetBinding(iconDrawing, GeometryDrawing.BrushProperty, brushBinding);

                var command = commandCtor.Invoke(new object[]
                {
                    OpenChangesOptionsMenu,
                    "Options",
                    new DrawingBrush
                    {                        
                        Drawing = iconDrawing,                        
                        Viewport = new Rect(0.1, 0.1, 0.8, 0.8),
                    },
                });

                list.Add(command);
                sectionCommandsProperty.SetValue(changesSection, list);
            }
        }

        void DoOpenOnGitHub()
        {
            var repo = Services.PackageServiceProvider.GetServiceSafe<ITeamExplorerServiceHolder>().ActiveRepo;
            var browser = Services.PackageServiceProvider.GetServiceSafe<IVisualStudioBrowser>();
            var url = repo.CloneUrl.ToRepositoryUrl().Append("pull/" + ViewModel.Model.Number);
            browser.OpenUrl(url);
        }

        void DoOpenChangesOptionsMenu(dynamic o)
        {
            var menu = changesSection.ContextMenu;
            var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
            var scaleX = g.DpiX / 96.0;
            var scaleY = g.DpiY / 96.0;
            menu.DataContext = DataContext;
            menu.Placement = PlacementMode.Absolute;
            menu.HorizontalOffset = o.MenuX / scaleX;
            menu.VerticalOffset = o.MenuY / scaleY;
            menu.IsOpen = true;
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
                switch (ViewModel.OpenChangedFileAction)
                {
                    case OpenChangedFileAction.Open:
                        DoOpenFile(file).Forget();
                        break;
                    case OpenChangedFileAction.Diff:
                        DoDiffFile(file).Forget();
                        break;
                }
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

        void ListView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ApplyContextMenuBinding<ListViewItem>(sender, e);
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
