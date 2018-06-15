using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    [ExportViewFor(typeof(IPullRequestListViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestListView : UserControl
    {
        IDisposable subscription;

        [ImportingConstructor]
        public PullRequestListView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) =>
            {
                var vm = DataContext as IPullRequestListViewModel;
                subscription?.Dispose();
                subscription = vm.AuthorFilter.WhenAnyValue(x => x.Selected)
                    .Skip(1)
                    .Subscribe(_ => authorFilterDropDown.IsOpen = false);
            };
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

        private void authorFilterDropDown_PopupOpened(object sender, EventArgs e)
        {
            authorFilter.FocusSearchBox();
        }
    }
}
