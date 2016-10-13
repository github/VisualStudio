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
                d(ViewModel.OpenFile.Subscribe(x => DoOpenFile((IPullRequestFileViewModel)x)));
                d(ViewModel.DiffFile.Subscribe(x => DoDiffFile((IPullRequestFileViewModel)x).Forget()));
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

                var command = commandCtor.Invoke(new object[]
                {
                    OpenChangesOptionsMenu,
                    "Options",
                    new DrawingBrush
                    {                        
                        Drawing = new GeometryDrawing
                        {
                            Brush = (Brush)FindResource("GitHubVsWindowText"),
                            Geometry = OcticonPath.GetGeometryForIcon(Octicon.three_bars),
                        },                        
                        Viewport = new Rect(0.1, 0.1, 0.8, 0.8),
                    },
                });

                list.Add(command);
                sectionCommandsProperty.SetValue(changesSection, list);
            }
        }

        void DoOpenOnGitHub()
        {
            var repo = Services.PackageServiceProvider.GetExportedValue<ITeamExplorerServiceHolder>().ActiveRepo;
            var browser = Services.PackageServiceProvider.GetExportedValue<IVisualStudioBrowser>();
            var url = repo.CloneUrl.ToRepositoryUrl().Append("pull/" + ViewModel.Number);
            browser.OpenUrl(url);
        }

        void DoOpenChangesOptionsMenu(dynamic o)
        {
            var menu = changesSection.ContextMenu;
            menu.DataContext = DataContext;
            menu.Placement = PlacementMode.Absolute;
            menu.HorizontalOffset = o.MenuX;
            menu.VerticalOffset = o.MenuY;
            menu.IsOpen = true;
        }

        void DoOpenFile(IPullRequestFileViewModel file)
        {
            if (ViewModel.CheckoutMode == CheckoutMode.UpToDate)
            {
                var fileName = ViewModel.GetFullPath(file);
                Services.Dte.ItemOperations.OpenFile(fileName);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "file")]
        async Task DoDiffFile(IPullRequestFileViewModel file)
        {
            if (ViewModel.CheckoutMode == CheckoutMode.UpToDate)
            {
                var fileNames = await ViewModel.GetFilesForDiff(file);
                var leftLabel = $"{file.FileName};{ViewModel.TargetBranchDisplayName}";
                var rightLabel = $"{file.FileName};PR {ViewModel.Number}";

                Services.DifferenceService.OpenComparisonWindow2(
                    fileNames.Item1,
                    fileNames.Item2,
                    $"{leftLabel} vs {rightLabel}",
                    file.Path,
                    leftLabel,
                    rightLabel,
                    string.Empty,
                    string.Empty,
                    0);
            }
        }

        private void FileListMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var file = (e.OriginalSource as FrameworkElement)?.DataContext as IPullRequestFileViewModel;

            if (file != null)
            {
                switch (ViewModel.OpenChangedFileAction)
                {
                    case OpenChangedFileAction.Open:
                        DoOpenFile(file);
                        break;
                    case OpenChangedFileAction.Diff:
                        DoDiffFile(file).Forget();
                        break;
                }
            }
        }

        private void FileListMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (e.OriginalSource as Visual)?.GetSelfAndVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();
            
            if (item != null)
            {
                // Select tree view item on right click.
                item.IsSelected = true;
            }
        }
    }
}
