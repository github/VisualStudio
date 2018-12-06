using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Input;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Info;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.Settings;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using GitHub.VisualStudio.UI;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Controls;

namespace GitHub.VisualStudio.TeamExplorer.Home
{
    [TeamExplorerSection(GitHubHomeSectionId, TeamExplorerPageIds.Home, 10)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubHomeSection : TeamExplorerSectionBase, IGitHubHomeSection
    {
        public const string GitHubHomeSectionId = "72008232-2104-4FA0-A189-61B0C6F91198";
        const string TrainingUrl = "https://services.github.com/on-demand/windows/visual-studio";
        readonly static Guid welcomeMessageGuid = new Guid(Guids.TeamExplorerWelcomeMessage);

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

            var openOnGitHub = new RelayCommand(_ => DoOpenOnGitHub());
            OpenOnGitHub = openOnGitHub;
        }

        bool IsGitToolsMessageVisible()
        {
            return teamExplorerServices.IsNotificationVisible(new Guid(Guids.TeamExplorerInstall3rdPartyGitTools));
        }

        protected async override void RepoChanged()
        {
            IsLoggedIn = true;
            IsVisible = false;

            base.RepoChanged();

            IsVisible = await IsAGitHubRepo(ActiveRepoUri);

            if (IsVisible)
            {
                RepoName = ActiveRepoName;
                RepoUrl = ActiveRepoUri.ToString();
                Icon = GetIcon(false, true, false);

                // We want to display a welcome message but only if Team Explorer isn't
                // already displaying the "Install 3rd Party Tools" message and the current repo is hosted on GitHub. 
                if (!settings.HideTeamExplorerWelcomeMessage && !IsGitToolsMessageVisible())
                {
                    ShowWelcomeMessage();
                }

                Debug.Assert(SimpleApiClient != null,
                    "If we're in this block, simpleApiClient cannot be null. It was created by UpdateStatus");
                var repo = await SimpleApiClient.GetRepository();
                Icon = GetIcon(repo.Private, true, repo.Fork);
                IsLoggedIn = await IsUserAuthenticated();
            }
            else
            {
                teamExplorerServices.HideNotification(welcomeMessageGuid);
            }
        }

        public override async void Refresh()
        {
            IsVisible = await IsAGitHubRepo(ActiveRepoUri);
            if (IsVisible)
            {
                IsLoggedIn = await IsUserAuthenticated();
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
            var dialogService = ServiceProvider.GetService<IDialogService>();
            dialogService.ShowLoginDialog();
        }

        void DoOpenOnGitHub()
        {
            visualStudioBrowser?.OpenUrl(ActiveRepo.CloneUrl.ToRepositoryUrl());
        }

        void ShowWelcomeMessage()
        {
            teamExplorerServices.ShowMessage(
                Resources.TeamExplorerWelcomeMessage,
                new RelayCommand(o =>
                {
                    var str = o.ToString();

                    switch (str)
                    {
                        case "show-training":
                            visualStudioBrowser.OpenUrl(new Uri(TrainingUrl));
                            usageTracker.IncrementCounter(x => x.NumberOfWelcomeTrainingClicks).Forget();
                            break;
                        case "show-docs":
                            visualStudioBrowser.OpenUrl(new Uri(GitHubUrls.Documentation));
                            usageTracker.IncrementCounter(x => x.NumberOfWelcomeDocsClicks).Forget();
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