using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GitHub.Exports;
using GitHub.UI.Helpers;
using GitHub.ViewModels.GitHubPane;
using GitHub.VisualStudio.UI.Helpers;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    [ExportViewFor(typeof(IPullRequestFilesViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestFilesView : UserControl
    {
        public PullRequestFilesView()
        {
            InitializeComponent();
            PreviewMouseWheel += ScrollViewerUtilities.FixMouseWheelScroll;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
        }

        void changesTree_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ApplyContextMenuBinding<TreeViewItem>(sender, e);
        }

        void changesTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var file = (e.OriginalSource as FrameworkElement)?.DataContext as IPullRequestFileNode;

            if (file != null)
            {
                (DataContext as IPullRequestFilesViewModel)?.DiffFile.Execute(file);
            }
        }

        void changesTree_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (e.OriginalSource as Visual)?.GetSelfAndVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();

            if (item != null)
            {
                // Select tree view item on right click.
                item.IsSelected = true;
            }
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
                    foreach (var menuItem in container.ContextMenu.Items.OfType<MenuItem>())
                    {
                        menuItem.CommandParameter = fileNode;
                    }

                    // HACK: MenuItem doesn't re-query ICommand.CanExecute when CommandParameter changes. Force
                    // this to happen by resetting its DataContext to null and then to the correct value.
                    container.ContextMenu.DataContext = null;
                    container.ContextMenu.DataContext = this.DataContext;
                    e.Handled = false;
                }
            }
        }

        private void changesTree_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var file = (e.OriginalSource as FrameworkElement)?.DataContext as IPullRequestFileNode;

                if (file != null)
                {
                    (DataContext as IPullRequestFilesViewModel)?.DiffFile.Execute(file);
                }
            }
        }
    }
}
