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
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows;
using System.Reactive.Disposables;

namespace GitHub.VisualStudio.UI.Views
{
    public class GenericPullRequestListView : ViewBase<IPullRequestListViewModel, PullRequestListView>
    { }

    [ExportView(ViewType = UIViewType.PRList)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestListView : GenericPullRequestListView, IDisposable
    {
        readonly Subject<int> open = new Subject<int>();
        readonly Subject<object> create = new Subject<object>();

        [ImportingConstructor]
        public PullRequestListView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
            });
        }

        bool disposed;
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposed)
            {
                if (disposing)
                {
                    open.Dispose();
                    create.Dispose();
                }

                disposed = true;
            }
        }
    }
}
