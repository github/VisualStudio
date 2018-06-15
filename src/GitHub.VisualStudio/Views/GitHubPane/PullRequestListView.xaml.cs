using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    [ExportViewFor(typeof(IPullRequestListViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestListView : UserControl
    {
        [ImportingConstructor]
        public PullRequestListView()
        {
            InitializeComponent();
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var control = sender as ListBoxItem;
            var pr = control?.DataContext as IPullRequestListItemViewModel;
            var vm = DataContext as IPullRequestListViewModel;

            if (pr != null && vm != null)
            {
                vm.OpenItem.Execute(pr);
            }
        }

        private void AuthorDropdown_PopupOpened(object sender, EventArgs e)
        {
            userFilter.FocusSearchBox();
        }
    }
}
