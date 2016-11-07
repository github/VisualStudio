using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
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
using GitHub.VisualStudio.UI.Helpers;
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

        private void FileListMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var file = (e.OriginalSource as FrameworkElement)?.DataContext as IPullRequestFileNode;

            if (file != null)
            {
                // TODO: Implement open/diff.
            }
        }
    }
}
