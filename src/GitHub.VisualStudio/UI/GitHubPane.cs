using System;
using System.Threading.Tasks;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using GitHub.Factories;
using GitHub.Services;
using GitHub.ViewModels.GitHubPane;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using ReactiveUI;
using Microsoft;

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
    public class GitHubPane : ToolWindowPane
    {
        public const string GitHubPaneGuid = "6b0fdc0a-f28e-47a0-8eed-cc296beff6d2";

        JoinableTask<IGitHubPaneViewModel> viewModelTask;

        IDisposable viewSubscription;
        ContentPresenter contentPresenter;

        public FrameworkElement View
        {
            get { return contentPresenter.Content as FrameworkElement; }
            set
            {
                viewSubscription?.Dispose();
                viewSubscription = null;

                contentPresenter.Content = value;

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

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GitHubPane() : base(null)
        {
            Caption = "GitHub";
            Content = contentPresenter = new ContentPresenter();

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
            // Using JoinableTaskFactory from parent AsyncPackage. That way if VS shuts down before this
            // work is done, we won't risk crashing due to arbitrary work going on in background threads.
            var asyncPackage = (AsyncPackage)Package;
            viewModelTask = asyncPackage.JoinableTaskFactory.RunAsync(() => InitializeAsync(asyncPackage));
        }

        public Task<IGitHubPaneViewModel> GetViewModelAsync() => viewModelTask.JoinAsync();

        async Task<IGitHubPaneViewModel> InitializeAsync(AsyncPackage asyncPackage)
        {
            try
            {
                ShowInitializing();

                // Allow MEF to initialize its cache asynchronously
                var provider = (IGitHubServiceProvider)await asyncPackage.GetServiceAsync(typeof(IGitHubServiceProvider));
                Assumes.Present(provider);

                var teServiceHolder = provider.GetService<ITeamExplorerServiceHolder>();
                teServiceHolder.ServiceProvider = this;

                var factory = provider.GetService<IViewViewModelFactory>();
                var viewModel = provider.ExportProvider.GetExportedValue<IGitHubPaneViewModel>();
                await viewModel.InitializeAsync(this);

                View = factory.CreateView<IGitHubPaneViewModel>();
                View.DataContext = viewModel;

                return viewModel;
            }
            catch (Exception e)
            {
                ShowError(e);
                throw;
            }
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
            ThreadHelper.ThrowIfNotOnUIThread();

            base.OnToolWindowCreated();

            Marshal.ThrowExceptionForHR(((IVsWindowFrame)Frame)?.SetProperty(
                (int)__VSFPROPID5.VSFPROPID_SearchPlacement,
                __VSSEARCHPLACEMENT.SP_STRETCH) ?? 0);

            var pane = View?.DataContext as IGitHubPaneViewModel;
            UpdateSearchHost(pane?.IsSearchEnabled ?? false, pane?.SearchQuery);
        }

        void ShowInitializing()
        {
            // This page is intentionally left blank.
        }

        void ShowError(Exception e)
        {
            View = new TextBox
            {
                Text = e.ToString(),
                IsReadOnly = true,
            };
        }

        void UpdateSearchHost(bool enabled, string query)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (SearchHost != null)
            {
                SearchHost.IsEnabled = enabled;

                var searchString = SearchHost.SearchQuery?.SearchString;
                if (searchString?.Trim() != query?.Trim())
                {
                    // SearchAsync will crash the process if we send it a duplicate string.
                    // There is a SearchTrimsWhitespace setting that makes searched with leading or trailing
                    // white-space appear as duplicates. We compare the query with trimmed white-space to avoid this.
                    // https://github.com/github/VisualStudio/issues/1948
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
                ThreadHelper.ThrowIfNotOnUIThread();

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
