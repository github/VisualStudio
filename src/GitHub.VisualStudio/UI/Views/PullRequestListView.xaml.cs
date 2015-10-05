using System.Windows;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.UI;
using GitHub.ViewModels;
using NullGuard;
using ReactiveUI;
using System.ComponentModel.Composition;

namespace GitHub.VisualStudio.UI.Views
{
    public class GenericPullRequestListView : SimpleViewUserControl<IPullRequestListViewModel, PullRequestListView>
    { }

    [ExportView(ViewType = UIViewType.PullRequestList)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestListView : GenericPullRequestListView
    {
        public PullRequestListView()
        {
            InitializeComponent();

            DataContextChanged += (s, e) => ViewModel = e.NewValue as IPullRequestListViewModel;
            this.WhenActivated(d =>
            {
                d(this.OneWayBind(ViewModel, vm => vm.PullRequests, v => v.pullRequests.ItemsSource));
                NotifyDone();
            });
        }
    }
}
