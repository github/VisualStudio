using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using GitHub.Controls;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.UI;
using GitHub.ViewModels;
using NullGuard;
using ReactiveUI;
using System.Windows.Media.Imaging;

namespace GitHub.VisualStudio.UI.Views
{
    [ExportView(ViewType = UIViewType.PullRequestList)]
    public partial class PullRequestList : SimpleViewUserControl, IViewFor<IPullRequestListViewModel>, IView
    {
        public PullRequestList()
        {
            InitializeComponent();

            DataContextChanged += (s, e) => ViewModel = e.NewValue as IPullRequestListViewModel;
            this.WhenActivated(d =>
            {
                d(this.OneWayBind(ViewModel, vm => vm.PullRequests, v => v.pullRequests.ItemsSource));
                NotifyDone();
            });
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(IPullRequestListViewModel), typeof(PullRequestList), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IPullRequestListViewModel)value; }
        }

        object IView.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IPullRequestListViewModel)value; }
        }

        public IPullRequestListViewModel ViewModel
        {
            [return: AllowNull]
            get { return (IPullRequestListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
