using System;
using System.ComponentModel.Composition;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Controls;
using GitHub.Services;
using GitHub.Api;
using GitHub.Primitives;
using System.Threading.Tasks;
using System.Diagnostics;
using GitHub.Extensions;
using System.Windows.Input;
using ReactiveUI;
using GitHub.VisualStudio.UI;
using GitHub.Settings;
using System.Windows.Threading;
using GitHub.Info;

namespace GitHub.VisualStudio.TeamExplorer.Home
{
    //[TeamExplorerSection(GitHubHomeSectionId, TeamExplorerPageIds.Home, 10)]
    //[PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubHomeSection : TeamExplorerSectionBase, IGitHubHomeSection
    {
        public const string GitHubHomeSectionId = "72008232-2104-4FA0-A189-61B0C6F91198";
        const string TrainingUrl = "https://services.github.com/on-demand/windows/visual-studio";

        readonly IVisualStudioBrowser visualStudioBrowser;
        readonly ITeamExplorerServices teamExplorerServices;
        readonly IPackageSettings settings;
        readonly IUsageTracker usageTracker;

        [ImportingConstructor]
        public GitHubHomeSection(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory,
            ITeamExplorerServiceHolder holder,
            IVisualStudioBrowser visualStudioBrowser,
            ITeamExplorerServices teamExplorerServices,
            IPackageSettings settings,
            IUsageTracker usageTracker)
            : base(serviceProvider, apiFactory, holder)
        {
            Title = "GitHub";
            View = new GitHubHomeContent();
            View.DataContext = this;
            this.visualStudioBrowser = visualStudioBrowser;
            this.teamExplorerServices = teamExplorerServices;
            this.settings = settings;
            this.usageTracker = usageTracker;

            var openOnGitHub = ReactiveCommand.Create();
            openOnGitHub.Subscribe(_ => DoOpenOnGitHub());
            OpenOnGitHub = openOnGitHub;

            // We want to display a welcome message but only if Team Explorer isn't
            // already displaying the "Install 3rd Party Tools" message. To do this
            // we need to set a timer and check in the tick as at this point the message
            // won't be initialized.
            if (!settings.HideTeamExplorerWelcomeMessage)
            {
                var timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(10);
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    if (!IsGitToolsMessageVisible())
                    {
                        ShowWelcomeMessage();
                    }
                };
                timer.Start();
            }
        }

        bool IsGitToolsMessageVisible()
        {
            return teamExplorerServices.IsNotificationVisible(new Guid(Guids.TeamExplorerInstall3rdPartyGitTools));
        }

        protected async override void RepoChanged(bool changed)
        {
            IsLoggedIn = true;
            IsVisible = false;

            base.RepoChanged(changed);

            IsVisible = await IsAGitHubRepo();

            if (IsVisible)
            {
                RepoName = ActiveRepoName;
                RepoUrl = ActiveRepoUri.ToString();
                Icon = GetIcon(false, true, false);
                Debug.Assert(SimpleApiClient != null,
                    "If we're in this block, simpleApiClient cannot be null. It was created by UpdateStatus");
                var repo = await SimpleApiClient.GetRepository();
                Icon = GetIcon(repo.Private, true, repo.Fork);
                IsLoggedIn = IsUserAuthenticated();
            }
        }

        public override async void Refresh()
        {
            IsVisible = await IsAGitHubRepo();
            if (IsVisible)
            {
                IsLoggedIn = IsUserAuthenticated();
            }
            base.Refresh();
        }

        static Octicon GetIcon(bool isPrivate, bool isHosted, bool isFork)
        {
            return !isHosted ? Octicon.device_desktop
                : isPrivate ? Octicon.@lock
                : isFork ? Octicon.repo_forked : Octicon.repo;
        }

        public void Login()
        {
            StartFlow(UIControllerFlow.Authentication);
        }

        void StartFlow(UIControllerFlow controllerFlow)
        {
            var notifications = ServiceProvider.TryGetService<INotificationDispatcher>();
            var teServices = ServiceProvider.TryGetService<ITeamExplorerServices>();
            notifications.AddListener(teServices);

            ServiceProvider.GitServiceProvider = TEServiceProvider;
            var uiProvider = ServiceProvider.TryGetService<IUIProvider>();
            var controller = uiProvider.Configure(controllerFlow);
            controller.ListenToCompletionState()
                .Subscribe(success =>
                {
                    Refresh();
                });
            uiProvider.RunInDialog(controller);

            notifications.RemoveListener();
        }

        void DoOpenOnGitHub()
        {
            visualStudioBrowser?.OpenUrl(ActiveRepo.CloneUrl.ToRepositoryUrl());
        }

        void ShowWelcomeMessage()
        {
            var welcomeMessageGuid = new Guid(Guids.TeamExplorerWelcomeMessage);
            teamExplorerServices.ShowMessage(
                Resources.TeamExplorerWelcomeMessage,
                new RelayCommand(o =>
                {
                    var str = o.ToString();

                    switch (str)
                    {
                        case "show-training":
                            visualStudioBrowser.OpenUrl(new Uri(TrainingUrl));
                            usageTracker.IncrementWelcomeTrainingClicks().Forget();
                            break;
                        case "show-docs":
                            visualStudioBrowser.OpenUrl(new Uri(GitHubUrls.Documentation));
                            usageTracker.IncrementWelcomeDocsClicks().Forget();
                            break;
                        case "dont-show-again":
                            teamExplorerServices.HideNotification(welcomeMessageGuid);
                            settings.HideTeamExplorerWelcomeMessage = true;
                            settings.Save();
                            break;
                    }
                }),
                false,
                welcomeMessageGuid);
        }

        protected GitHubHomeContent View
        {
            get { return SectionContent as GitHubHomeContent; }
            set { SectionContent = value; }
        }

        string repoName = String.Empty;
        public string RepoName
        {
            get { return repoName; }
            set { repoName = value; this.RaisePropertyChange(); }
        }

        string repoUrl = String.Empty;
        public string RepoUrl
        {
            get { return repoUrl; }
            set { repoUrl = value; this.RaisePropertyChange(); }
        }

        Octicon icon;
        public Octicon Icon
        {
            get { return icon; }
            set { icon = value; this.RaisePropertyChange(); }
        }

        bool isLoggedIn;
        public bool IsLoggedIn
        {
            get { return isLoggedIn; }
            set { isLoggedIn = value; this.RaisePropertyChange(); }
        }

        public ICommand OpenOnGitHub { get; }
    }
}