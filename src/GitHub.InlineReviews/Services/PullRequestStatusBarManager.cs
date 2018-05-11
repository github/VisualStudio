using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Linq.Expressions;
using GitHub.Commands;
using GitHub.InlineReviews.Views;
using GitHub.InlineReviews.ViewModels;
using GitHub.Services;
using GitHub.Models;
using GitHub.Logging;
using GitHub.Extensions;
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

        IDisposable currentSessionSubscription;

        [ImportingConstructor]
        public PullRequestStatusBarManager(
            Lazy<IUsageTracker> usageTracker,
            IOpenPullRequestsCommand openPullRequestsCommand,
            IShowCurrentPullRequestCommand showCurrentPullRequestCommand,
            Lazy<IPullRequestSessionManager> pullRequestSessionManager,
            Lazy<ITeamExplorerContext> teamExplorerContext)
        {
            this.openPullRequestsCommand = new UsageTrackingCommand(openPullRequestsCommand,
                usageTracker, x => x.NumberOfStatusBarOpenPullRequestList);
            this.showCurrentPullRequestCommand = new UsageTrackingCommand(showCurrentPullRequestCommand,
                usageTracker, x => x.NumberOfShowCurrentPullRequest);

            this.pullRequestSessionManager = pullRequestSessionManager;
            this.teamExplorerContext = teamExplorerContext;
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
                teamExplorerContext.Value.WhenAnyValue(x => x.ActiveRepository)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => RefreshActiveRepository(x));
            }
            catch (Exception e)
            {
                log.Error(e, "Error initializing");
            }
        }

        void RefreshActiveRepository(ILocalRepositoryModel repository)
        {
            currentSessionSubscription?.Dispose();
            currentSessionSubscription = pullRequestSessionManager.Value.WhenAnyValue(x => x.CurrentSession)
                .Subscribe(x => RefreshCurrentSession(repository, x));
        }

        void RefreshCurrentSession(ILocalRepositoryModel repository, IPullRequestSession session)
        {
            var cloneUrl = repository?.CloneUrl;
            if (cloneUrl != null)
            {
                // Only show PR status bar if repo has remote
                var viewModel = CreatePullRequestStatusViewModel(session);
                ShowStatus(viewModel);
            }
            else
            {
                ShowStatus(null);
            }
        }

        PullRequestStatusViewModel CreatePullRequestStatusViewModel(IPullRequestSession session)
        {
            var pullRequestStatusViewModel = new PullRequestStatusViewModel(openPullRequestsCommand, showCurrentPullRequestCommand);
            var pullRequest = session?.PullRequest;
            pullRequestStatusViewModel.Number = pullRequest?.Number;
            pullRequestStatusViewModel.Title = pullRequest?.Title;
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

        class UsageTrackingCommand : ICommand
        {
            readonly ICommand command;
            readonly Lazy<IUsageTracker> usageTracker;
            readonly Expression<Func<UsageModel.MeasuresModel, int>> counter;

            internal UsageTrackingCommand(ICommand command, Lazy<IUsageTracker> usageTracker,
                Expression<Func<UsageModel.MeasuresModel, int>> counter)
            {
                this.command = command;
                this.usageTracker = usageTracker;
                this.counter = counter;
            }

            public event EventHandler CanExecuteChanged
            {
                add { command.CanExecuteChanged += value; }
                remove { command.CanExecuteChanged -= value; }
            }

            public bool CanExecute(object parameter)
            {
                return command.CanExecute(parameter);
            }

            public void Execute(object parameter)
            {
                command.Execute(parameter);
                usageTracker.Value.IncrementCounter(counter).Forget();
            }
        }
    }
}
