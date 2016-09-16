using System;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.UI;
using GitHub.ViewModels;
using ReactiveUI;
using System.ComponentModel.Composition;
using System.Reactive.Subjects;
using System.Windows.Input;
using GitHub.Services;
using GitHub.Primitives;

namespace GitHub.VisualStudio.UI.Views
{
    public class GenericPullRequestListView : SimpleViewUserControl<IPullRequestListViewModel, PullRequestListView>
    { }

    [ExportView(ViewType = UIViewType.PRList)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestListView : GenericPullRequestListView, IHasDetailView, IHasCreationView
    {
        readonly Subject<ViewWithData> open = new Subject<ViewWithData>();
        readonly Subject<ViewWithData> create = new Subject<ViewWithData>();

        public PullRequestListView()
        {
            InitializeComponent();

            OpenPR = new RelayCommand(x =>
            {
                NotifyOpen(new ViewWithData { Data = x });
            });

            CreatePR = new RelayCommand(x => NotifyCreate());

            this.WhenActivated(d =>
            {
            });
        }

        public IObservable<ViewWithData> Open { get { return open; } }
        public IObservable<ViewWithData> Create { get { return create; } }
        public ICommand OpenPR { get; set; }
        public ICommand CreatePR { get; set; }
        protected void NotifyOpen(ViewWithData id)
        {
            open.OnNext(id);
            open.OnCompleted();
        }

        protected void NotifyCreate()
        {
            create.OnNext(null);
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
