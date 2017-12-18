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
using Microsoft.VisualStudio.Shell;
using Serilog;

namespace GitHub.InlineReviews.Services
{
    [Export(typeof(IPullRequestStatusManager))]
    public class PullRequestStatusManager : IPullRequestStatusManager
    {
        static readonly ILogger log = LogManager.ForContext<PullRequestStatusManager>();
        const string StatusBarPartName = "PART_SccStatusBarHost";

        readonly SVsServiceProvider serviceProvider;
        readonly Window mainWindow;
        readonly IPullRequestSessionManager pullRequestSessionManager;
        readonly IUsageTracker usageTracker;

        [ImportingConstructor]
        public PullRequestStatusManager(SVsServiceProvider serviceProvider, IPullRequestSessionManager pullRequestSessionManager, IUsageTracker usageTracker)
            : this()
        {
            this.serviceProvider = serviceProvider;
            this.pullRequestSessionManager = pullRequestSessionManager;
            this.usageTracker = usageTracker;
        }

        public PullRequestStatusManager()
        {
            mainWindow = Application.Current.MainWindow;
        }

        public void Initialize()
        {
            RefreshCurrentSession();
            pullRequestSessionManager.PropertyChanged += PullRequestSessionManager_PropertyChanged;
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
            var command = new RaisePullRequestCommand(serviceProvider, usageTracker);
            var pullRequestStatusViewModel = new PullRequestStatusViewModel(command);
            pullRequestStatusViewModel.Number = pullRequest.Number;
            pullRequestStatusViewModel.Title = pullRequest.Title;
            return pullRequestStatusViewModel;
        }

        void ShowStatus(PullRequestStatusViewModel pullRequestStatusViewModel = null)
        {
            var statusBar = FindSccStatusBar();
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

        StatusBar FindSccStatusBar()
        {
            var contentControl = mainWindow?.Template?.FindName(StatusBarPartName, mainWindow) as ContentControl;
            return contentControl?.Content as StatusBar;
        }

        class RaisePullRequestCommand : ICommand
        {
            readonly string guid = Guids.guidGitHubCmdSetString;
            readonly int id = PkgCmdIDList.showCurrentPullRequestCommand;

            readonly IServiceProvider serviceProvider;
            readonly IUsageTracker usageTracker;

            internal RaisePullRequestCommand(IServiceProvider serviceProvider, IUsageTracker usageTracker)
            {
                this.serviceProvider = serviceProvider;
                this.usageTracker = usageTracker;
            }

            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter)
            {
                try
                {
                    var dte = serviceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
                    object customIn = null;
                    object customOut = null;
                    dte.Commands.Raise(guid, id, ref customIn, ref customOut);
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
