using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.ComponentModel.Composition;
using GitHub.InlineReviews.Views;
using GitHub.InlineReviews.ViewModels;
using GitHub.Services;
using GitHub.VisualStudio;
using GitHub.Models;
using GitHub.Logging;
using GitHub.Extensions;
using Serilog;

namespace GitHub.InlineReviews.Services
{
    public class PullRequestStatusBarManager
    {
        static readonly ILogger log = LogManager.ForContext<PullRequestStatusBarManager>();
        const string StatusBarPartName = "PART_SccStatusBarHost";

        readonly IVSGitExt gitExt;
        readonly IUsageTracker usageTracker;
        readonly IGitHubServiceProvider serviceProvider;

        IPullRequestSessionManager pullRequestSessionManager;

        [ImportingConstructor]
        public PullRequestStatusBarManager(IVSGitExt gitExt, IUsageTracker usageTracker, IGitHubServiceProvider serviceProvider)
        {
            this.gitExt = gitExt;
            this.usageTracker = usageTracker;
            this.serviceProvider = serviceProvider;

            OnActiveRepositoriesChanged();
            gitExt.ActiveRepositoriesChanged += OnActiveRepositoriesChanged;
        }

        void OnActiveRepositoriesChanged()
        {
            if (gitExt.ActiveRepositories.Count > 0)
            {
                gitExt.ActiveRepositoriesChanged -= OnActiveRepositoriesChanged;
                Application.Current.Dispatcher.Invoke(() => StartShowingStatus());
            }
        }

        void StartShowingStatus()
        {
            try
            {
                // Create just in time on Main thread.
                pullRequestSessionManager = serviceProvider.GetService<IPullRequestSessionManager>();

                RefreshCurrentSession();
                pullRequestSessionManager.PropertyChanged += PullRequestSessionManager_PropertyChanged;
            }
            catch (Exception e)
            {
                log.Error(e, "Error initializing");
            }
        }

        void PullRequestSessionManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PullRequestSessionManager.CurrentSession))
            {
                RefreshCurrentSession();
            }
        }

        void RefreshCurrentSession()
        {
            var pullRequest = pullRequestSessionManager.CurrentSession?.PullRequest;
            var viewModel = pullRequest != null ? CreatePullRequestStatusViewModel(pullRequest) : null;
            ShowStatus(viewModel);
        }

        PullRequestStatusViewModel CreatePullRequestStatusViewModel(IPullRequestModel pullRequest)
        {
            var dte = serviceProvider.TryGetService<EnvDTE.DTE>();
            var command = new RaisePullRequestCommand(dte, usageTracker);
            var pullRequestStatusViewModel = new PullRequestStatusViewModel(command);
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

        class RaisePullRequestCommand : ICommand
        {
            readonly string guid = Guids.guidGitHubCmdSetString;
            readonly int id = PkgCmdIDList.showCurrentPullRequestCommand;

            readonly EnvDTE.DTE dte;
            readonly IUsageTracker usageTracker;

            internal RaisePullRequestCommand(EnvDTE.DTE dte, IUsageTracker usageTracker)
            {
                this.dte = dte;
                this.usageTracker = usageTracker;
            }

            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter)
            {
                try
                {
                    object customIn = null;
                    object customOut = null;
                    dte?.Commands.Raise(guid, id, ref customIn, ref customOut);
                }
                catch (Exception e)
                {
                    log.Error(e, "Couldn't raise {Guid}:{ID}", guid, id);
                }

                usageTracker.IncrementCounter(x => x.NumberOfShowCurrentPullRequest).Forget();
            }

            public event EventHandler CanExecuteChanged
            {
                add { }
                remove { }
            }
        }
    }
}
