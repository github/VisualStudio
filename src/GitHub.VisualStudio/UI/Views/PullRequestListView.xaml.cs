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

        public PullRequestListView()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public PullRequestListView(IGitHubServiceProvider serviceProvider)
        {
            InitializeComponent();

            OpenPROnGitHub = new RelayCommand(x =>
            {
                var repo = serviceProvider.TryGetService<ITeamExplorerServiceHolder>()?.ActiveRepo;
                var browser = serviceProvider.TryGetService<IVisualStudioBrowser>();
                Debug.Assert(repo != null, "No active repo, cannot open PR on GitHub");
                Debug.Assert(browser != null, "No browser service, cannot open PR on GitHub");
                if (repo == null || browser == null)
                {
                    return;
                }
                var url = repo.CloneUrl.ToRepositoryUrl().Append("pull/" + x);
                browser.OpenUrl(url);
            });

            this.WhenActivated(d =>
            {
            });
        }

        public ICommand OpenPROnGitHub { get; set; }

        bool disposed;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposed) return;

            open.Dispose();
            create.Dispose();
            disposed = true;
        }
    }
}
