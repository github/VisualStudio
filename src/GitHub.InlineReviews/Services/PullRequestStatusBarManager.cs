using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Commands;
using GitHub.Extensions;
using GitHub.Primitives;
using GitHub.VisualStudio;
using GitHub.InlineReviews.Views;
using GitHub.InlineReviews.ViewModels;
using GitHub.Services;
using GitHub.Models;
using GitHub.Logging;
using Serilog;
using ReactiveUI;

namespace GitHub.InlineReviews.Services
{
    /// <summary>
    /// Manage the UI that shows the PR for the current branch.
    /// </summary>
    [Export(typeof(PullRequestStatusBarManager))]
    public class PullRequestStatusBarManager
    {
        static readonly ILogger log = LogManager.ForContext<PullRequestStatusBarManager>();
        const string StatusBarPartName = "PART_SccStatusBarHost";

        readonly ICommand openPullRequestsCommand;
        readonly ICommand showCurrentPullRequestCommand;

        // At the moment these must be constructed on the main thread.
        // TeamExplorerContext needs to retrieve DTE using GetService.
        readonly Lazy<IPullRequestSessionManager> pullRequestSessionManager;
        readonly Lazy<ITeamExplorerContext> teamExplorerContext;
        readonly Lazy<IConnectionManager> connectionManager;
        readonly Lazy<ITippingService> tippingService;

        [ImportingConstructor]
        public PullRequestStatusBarManager(
            Lazy<IUsageTracker> usageTracker,
            IOpenPullRequestsCommand openPullRequestsCommand,
            IShowCurrentPullRequestCommand showCurrentPullRequestCommand,
            Lazy<IPullRequestSessionManager> pullRequestSessionManager,
            Lazy<ITeamExplorerContext> teamExplorerContext,
            Lazy<IConnectionManager> connectionManager,
            Lazy<ITippingService> tippingService)
        {
            this.openPullRequestsCommand = new UsageTrackingCommand(usageTracker,
                x => x.NumberOfStatusBarOpenPullRequestList, openPullRequestsCommand);
            this.showCurrentPullRequestCommand = new UsageTrackingCommand(usageTracker,
                x => x.NumberOfShowCurrentPullRequest, showCurrentPullRequestCommand);

            this.pullRequestSessionManager = pullRequestSessionManager;
            this.teamExplorerContext = teamExplorerContext;
            this.connectionManager = connectionManager;
            this.tippingService = tippingService;
        }

        /// <summary>
        /// Start showing the PR for the active branch on the status bar.
        /// </summary>
        /// <remarks>
        /// This must be called from the Main thread.
        /// </remarks>
        public void StartShowingStatus()
        {
            try
            {
                var activeReposities = teamExplorerContext.Value.WhenAnyValue(x => x.ActiveRepository);
                var sessions = pullRequestSessionManager.Value.WhenAnyValue(x => x.CurrentSession);
                activeReposities
                    .CombineLatest(sessions, (r, s) => (r, s))
                    .Throttle(TimeSpan.FromSeconds(1))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => RefreshCurrentSession(x.r, x.s).Forget(log));
            }
            catch (Exception e)
            {
                log.Error(e, "Error initializing");
            }
        }

        async Task RefreshCurrentSession(LocalRepositoryModel repository, IPullRequestSession session)
        {
            if (repository != null && repository.HasRemotesButNoOrigin)
            {
                NoRemoteOriginCallout();
            }

            var showStatus = await IsDotComOrEnterpriseRepository(repository);
            if (!showStatus)
            {
                ShowStatus(null);
                return;
            }

            var viewModel = CreatePullRequestStatusViewModel(session);
            ShowStatus(viewModel);
        }

        [STAThread]
        void NoRemoteOriginCallout()
        {
            try
            {
                var view = FindSccStatusBar(Application.Current.MainWindow);
                if (view == null)
                {
                    log.Warning("Couldn't find SccStatusBar");
                    return;
                }

                tippingService.Value.RequestCalloutDisplay(
                    calloutId: Guids.NoRemoteOriginCalloutId,
                    title: Resources.CantFindGitHubUrlForRepository,
                    message: Resources.RepositoriesMustHaveRemoteOrigin,
                    isPermanentlyDismissible: true,
                    targetElement: view,
                    vsCommandGroupId: Guids.guidGitHubCmdSet,
                    vsCommandId: PkgCmdIDList.showGitHubPaneCommand);
            }
            catch (Exception e)
            {
                log.Error(e, nameof(NoRemoteOriginCallout));
            }
        }

        async Task<bool> IsDotComOrEnterpriseRepository(LocalRepositoryModel repository)
        {
            var cloneUrl = repository?.CloneUrl;
            if (cloneUrl == null)
            {
                // No active repository or remote
                return false;
            }

            var isDotCom = HostAddress.IsGitHubDotComUri(cloneUrl.ToRepositoryUrl());
            if (isDotCom)
            {
                // This is a github.com repository
                return true;
            }

            var connection = await connectionManager.Value.GetConnection(repository);
            if (connection != null)
            {
                // This is an enterprise repository
                return true;
            }

            return false;
        }

        PullRequestStatusViewModel CreatePullRequestStatusViewModel(IPullRequestSession session)
        {
            var pullRequestStatusViewModel = new PullRequestStatusViewModel(openPullRequestsCommand, showCurrentPullRequestCommand);
            var pullRequest = session?.PullRequest;
            pullRequestStatusViewModel.Number = pullRequest?.Number;
            pullRequestStatusViewModel.Title = pullRequest?.Title;
            return pullRequestStatusViewModel;
        }

        PullRequestStatusView ShowStatus(PullRequestStatusViewModel pullRequestStatusViewModel = null)
        {
            var statusBar = FindSccStatusBar(Application.Current.MainWindow);
            if (statusBar != null)
            {
                var githubStatusBar = Find<PullRequestStatusView>(statusBar);
                if (githubStatusBar != null)
                {
                    // Replace to ensure status shows up.
                    statusBar.Items.Remove(githubStatusBar);
                }

                if (pullRequestStatusViewModel != null)
                {
                    githubStatusBar = new PullRequestStatusView { DataContext = pullRequestStatusViewModel };
                    statusBar.Items.Insert(0, githubStatusBar);
                    return githubStatusBar;
                }
            }

            return null;
        }

        static T Find<T>(StatusBar statusBar)
        {
            foreach (var item in statusBar.Items)
            {
                if (item is T)
                {
                    return (T)item;
                }
            }

            return default(T);
        }

        StatusBar FindSccStatusBar(Window mainWindow)
        {
            var contentControl = mainWindow?.Template?.FindName(StatusBarPartName, mainWindow) as ContentControl;
            return contentControl?.Content as StatusBar;
        }
    }
}
