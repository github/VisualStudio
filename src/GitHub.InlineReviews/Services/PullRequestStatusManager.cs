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
using Microsoft.VisualStudio.Shell;

namespace GitHub.InlineReviews.Services
{
    [Export(typeof(IPullRequestStatusManager))]
    public class PullRequestStatusManager : IPullRequestStatusManager
    {
        const string StatusBarPartName = "PART_SccStatusBarHost";

        readonly Window mainWindow;
        readonly IPullRequestSessionManager pullRequestSessionManager;
        readonly PullRequestStatusViewModel pullRequestStatusViewModel;

        [ImportingConstructor]
        public PullRequestStatusManager(SVsServiceProvider serviceProvider, IPullRequestSessionManager pullRequestSessionManager)
            : this(CreatePullRequestStatusViewModel(serviceProvider))
        {
            this.pullRequestSessionManager = pullRequestSessionManager;

            RefreshCurrentSession();
            pullRequestSessionManager.PropertyChanged += PullRequestSessionManager_PropertyChanged;
        }

        public PullRequestStatusManager(PullRequestStatusViewModel pullRequestStatusViewModel)
        {
            this.pullRequestStatusViewModel = pullRequestStatusViewModel;
            mainWindow = Application.Current.MainWindow;
        }

        public void ShowStatus()
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

                githubStatusBar = new PullRequestStatusView { DataContext = pullRequestStatusViewModel };
                statusBar.Items.Insert(0, githubStatusBar);
            }
        }

        public void HideStatus()
        {
            var statusBar = FindSccStatusBar();
            if (statusBar != null)
            {
                var githubStatusBar = Find<PullRequestStatusView>(statusBar);
                if (githubStatusBar != null)
                {
                    statusBar.Items.Remove(githubStatusBar);
                }
            }
        }

        static PullRequestStatusViewModel CreatePullRequestStatusViewModel(IServiceProvider serviceProvider)
        {
            var command = new RaiseVsCommand(serviceProvider, Guids.guidGitHubCmdSetString, PkgCmdIDList.showCurrentPullRequestCommand);
            return new PullRequestStatusViewModel(command);
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
            pullRequestStatusViewModel.Number = pullRequest?.Number;
            pullRequestStatusViewModel.Title = pullRequest?.Title;
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

        class RaiseVsCommand : ICommand
        {
            readonly IServiceProvider serviceProvider;
            readonly string guid;
            readonly int id;

            internal RaiseVsCommand(IServiceProvider serviceProvider, string guid, int id)
            {
                this.serviceProvider = serviceProvider;
                this.guid = guid;
                this.id = id;
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
                    VsOutputLogger.WriteLine("Couldn't raise {0}: {1}", nameof(PkgCmdIDList.openPullRequestsCommand), e);
                    System.Diagnostics.Trace.WriteLine(e);
                }
            }

            public event EventHandler CanExecuteChanged;
        }

        class PullRequestStatusManagerInstaller
        {
            [STAThread]
            void Install()
            {
                var viewModel = new PullRequestStatusViewModel(null) { Number = 666, Title = "A beast of a PR" };
                var provider = new PullRequestStatusManager(viewModel);
                provider.HideStatus();
                provider.ShowStatus();
            }
        }
    }
}
