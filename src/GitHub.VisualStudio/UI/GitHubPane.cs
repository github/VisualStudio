using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using GitHub.Helpers;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Services;
using GitHub.ViewModels;
using GitHub.ViewModels.GitHubPane;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.ComponentModelHost;
using ReactiveUI;
using Task = System.Threading.Tasks.Task;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;

namespace GitHub.VisualStudio.UI
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid(GitHubPaneGuid)]
    public class GitHubPane : ToolWindowPane, IServiceProviderAware
    {
        public const string GitHubPaneGuid = "6b0fdc0a-f28e-47a0-8eed-cc296beff6d2";
        bool initialized = false;
        IDisposable viewSubscription;
        IGitHubPaneViewModel viewModel;
        ContentControl contentControl;

        public FrameworkElement View
        {
            get { return contentControl.Content as FrameworkElement; }
            set
            {
                viewSubscription?.Dispose();
                viewSubscription = null;

                contentControl.Content = value;

                viewSubscription = value.WhenAnyValue(x => x.DataContext)
                    .SelectMany(x =>
                    {
                        var pane = x as IGitHubPaneViewModel;
                        return pane?.WhenAnyValue(p => p.IsSearchEnabled, p => p.SearchQuery)
                            ?? Observable.Return(Tuple.Create<bool, string>(false, null));
                    })
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => UpdateSearchHost(x.Item1, x.Item2));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GitHubPane() : base(null)
        {
            Caption = "GitHub";
            Content = contentControl = new ContentControl();

            BitmapImageMoniker = new Microsoft.VisualStudio.Imaging.Interop.ImageMoniker()
            {
                Guid = Guids.guidImageMoniker,
                Id = 1
            };
            ToolBar = new CommandID(Guids.guidGitHubToolbarCmdSet, PkgCmdIDList.idGitHubToolbar);
            ToolBarLocation = (int)VSTWT_LOCATION.VSTWT_TOP;
        }

        public override bool SearchEnabled => true;

        protected override void Initialize()
        {
            base.Initialize();
            Initialize(this);
        }

        public void Initialize(IServiceProvider serviceProvider)
        {
            if (!initialized)
            {
                InitializeAsync(serviceProvider).Forget();
            }
        }

        async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            // Allow MEF to refresh its cache on a background thread so it isn't counted against us.
            var asyncServiceProvider = (IAsyncServiceProvider)GetService(typeof(SAsyncServiceProvider));
            await asyncServiceProvider.GetServiceAsync(typeof(SComponentModel));

            await ThreadingHelper.SwitchToMainThreadAsync();

            var provider = VisualStudio.Services.GitHubServiceProvider;
            var teServiceHolder = provider.GetService<ITeamExplorerServiceHolder>();
            teServiceHolder.ServiceProvider = serviceProvider;

            var factory = provider.GetService<IViewViewModelFactory>();
            viewModel = provider.ExportProvider.GetExportedValue<IGitHubPaneViewModel>();
            viewModel.InitializeAsync(this).Forget();

            View = factory.CreateView<IGitHubPaneViewModel>();
            View.DataContext = viewModel;
        }

        [SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods", Justification = "WTF CA, I'm overriding!")]
        public override IVsSearchTask CreateSearch(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback)
        {
            var pane = View?.DataContext as IGitHubPaneViewModel;

            if (pane != null)
            {
                return new SearchTask(pane, dwCookie, pSearchQuery, pSearchCallback);
            }

            return null;
        }

        public override void ClearSearch()
        {
            var pane = View?.DataContext as IGitHubPaneViewModel;

            if (pane != null)
            {
                pane.SearchQuery = null;
            }
        }

        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();

            Marshal.ThrowExceptionForHR(((IVsWindowFrame)Frame)?.SetProperty(
                (int)__VSFPROPID5.VSFPROPID_SearchPlacement,
                __VSSEARCHPLACEMENT.SP_STRETCH) ?? 0);

            var pane = View?.DataContext as IGitHubPaneViewModel;
            UpdateSearchHost(pane?.IsSearchEnabled ?? false, pane?.SearchQuery);
        }

        void UpdateSearchHost(bool enabled, string query)
        {
            if (SearchHost != null)
            {
                SearchHost.IsEnabled = enabled;

                if (SearchHost.SearchQuery?.SearchString != query)
                {
                    SearchHost.SearchAsync(query != null ? new SearchQuery(query) : null);
                }
            }
        }

        class SearchTask : VsSearchTask
        {
            readonly IGitHubPaneViewModel viewModel;

            public SearchTask(
                IGitHubPaneViewModel viewModel,
                uint dwCookie,
                IVsSearchQuery pSearchQuery,
                IVsSearchCallback pSearchCallback)
                : base(dwCookie, pSearchQuery, pSearchCallback)
            {
                this.viewModel = viewModel;
            }

            protected override void OnStartSearch()
            {
                viewModel.SearchQuery = SearchQuery.SearchString;
                base.OnStartSearch();
            }

            protected override void OnStopSearch() => viewModel.SearchQuery = null;
        }

        class SearchQuery : IVsSearchQuery
        {
            public SearchQuery(string query)
            {
                SearchString = query;
            }

            public uint ParseError => 0;
            public string SearchString { get; }

            public uint GetTokens(uint dwMaxTokens, IVsSearchToken[] rgpSearchTokens) => 0;
        }
    }
}
