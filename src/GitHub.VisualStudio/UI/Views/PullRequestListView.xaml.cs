using System;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.UI;
using GitHub.ViewModels;
using ReactiveUI;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Controls;
using System.Windows.Input;

namespace GitHub.VisualStudio.UI.Views
{
    public class GenericPullRequestListView : SimpleViewUserControl<IPullRequestListViewModel, PullRequestListView>
    { }

    [ExportView(ViewType = UIViewType.PRList)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestListView : GenericPullRequestListView, IHasDetailView, IHasCreationView
    {
        readonly Subject<object> open = new Subject<object>();
        readonly Subject<object> create = new Subject<object>();

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


        public IObservable<object> Open { get { return open; } }
        public IObservable<object> Create { get { return create; } }

        protected void NotifyOpen()
        {
            open.OnNext(null);
            open.OnCompleted();
        }

        protected void NotifyCreate()
        {
            create.OnNext(null);
            create.OnCompleted();
        }

        bool disposed;
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (disposed) return;

                open.Dispose();
                create.Dispose();
                disposed = true;
            }
        }
    }
}
