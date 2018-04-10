using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.ComponentModel.Composition;
using GitHub.Commands;
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

        readonly IOpenPullRequestsCommand openPullRequestsCommand;
        readonly IShowCurrentPullRequestCommand showCurrentPullRequestCommand;

        // At the moment this must be constructed on the main thread.
        // TeamExplorerContext needs to retrieve DTE using GetService.
        readonly Lazy<IPullRequestSessionManager> pullRequestSessionManager;

        [ImportingConstructor]
        public PullRequestStatusBarManager(
            IOpenPullRequestsCommand openPullRequestsCommand,
            IShowCurrentPullRequestCommand showCurrentPullRequestCommand,
            Lazy<IPullRequestSessionManager> pullRequestSessionManager)
        {
            this.openPullRequestsCommand = openPullRequestsCommand;
            this.showCurrentPullRequestCommand = showCurrentPullRequestCommand;
            this.pullRequestSessionManager = pullRequestSessionManager;
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
                pullRequestSessionManager.Value.WhenAnyValue(x => x.CurrentSession)
                    .Subscribe(x => RefreshCurrentSession());
            }
            catch (Exception e)
            {
                log.Error(e, "Error initializing");
            }
        }

        void RefreshCurrentSession()
        {
            var pullRequest = pullRequestSessionManager.Value.CurrentSession?.PullRequest;
            var viewModel = pullRequest != null ? CreatePullRequestStatusViewModel(pullRequest) : null;
            ShowStatus(viewModel);
        }

        PullRequestStatusViewModel CreatePullRequestStatusViewModel(IPullRequestModel pullRequest)
        {
            var pullRequestStatusViewModel = new PullRequestStatusViewModel(openPullRequestsCommand, showCurrentPullRequestCommand);
            pullRequestStatusViewModel.Number = pullRequest.Number;
            pullRequestStatusViewModel.Title = pullRequest.Title;
            return pullRequestStatusViewModel;
        }

        void ShowStatus(PullRequestStatusViewModel pullRequestStatusViewModel = null)
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
                }
            }
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
