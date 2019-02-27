using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Info;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.UI;
using GitHub.ViewModels;
using GitHub.ViewModels.TeamExplorer;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using GitHub.VisualStudio.UI;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio;
using ReactiveUI;

namespace GitHub.VisualStudio.TeamExplorer.Sync
{
    [TeamExplorerSection(GitHubPublishSectionId, TeamExplorerPageIds.GitCommits, 10)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubPublishSection : TeamExplorerSectionBase, IGitHubInvitationSection
    {
        public const string GitHubPublishSectionId = "92655B52-360D-4BF5-95C5-D9E9E596AC76";

        readonly Lazy<IVisualStudioBrowser> lazyBrowser;
        readonly IDialogService dialogService;
        bool loggedIn;

        [ImportingConstructor]
        public GitHubPublishSection(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder,
            IConnectionManager cm, Lazy<IVisualStudioBrowser> browser,
            IDialogService dialogService)
            : base(serviceProvider, apiFactory, holder, cm)
        {

            lazyBrowser = browser;
            this.dialogService = dialogService;
            Title = Resources.GitHubPublishSectionTitle;
            Name = "GitHub";
            Provider = "GitHub, Inc";
            Description = Resources.BlurbText;
            ShowLogin = false;
            ShowSignup = false;
            ShowGetStarted = false;
            IsVisible = false;
            IsExpanded = true;
            InitializeSectionView();
        }

        void InitializeSectionView()
        {
            var view = new GitHubInvitationContent();
            SectionContent = view;
            view.DataContext = this;
        }

        async void Setup()
        {
            if (ActiveRepo != null && ActiveRepoUri == null)
            {
                IsVisible = true;
                ShowGetStarted = true;
                loggedIn = await ConnectionManager.IsLoggedIn();
                ShowLogin = !loggedIn;
                ShowSignup = !loggedIn;
            }
            else
                IsVisible = false;
        }

        public override void Initialize(IServiceProvider serviceProvider)
        {
            base.Initialize(serviceProvider);
            Setup();
        }

        protected override void RepoChanged()
        {
            base.RepoChanged();
            Setup();
            InitializeSectionView();
        }

        public async Task Connect()
        {
            loggedIn = await ConnectionManager.IsLoggedIn();
            if (loggedIn)
                ShowPublish();
            else
                await Login();
        }

        public void SignUp()
        {
            OpenInBrowser(lazyBrowser, GitHubUrls.Plans);
        }

        public void ShowPublish()
        {
            var factory = ServiceProvider.GetService<IViewViewModelFactory>();
            var viewModel = ServiceProvider.GetService<IRepositoryPublishViewModel>();
            var busy = viewModel.WhenAnyValue(x => x.IsBusy).Subscribe(x => IsBusy = x);
            var completed = viewModel.PublishRepository
                .Where(x => x == ProgressState.Success)
                .Subscribe(_ =>
                {
                    ServiceProvider.TryGetService<ITeamExplorer>()?.NavigateToPage(new Guid(TeamExplorerPageIds.Home), null);
                    HandleCreatedRepo(ActiveRepo);
                });

            var view = factory.CreateView<IRepositoryPublishViewModel>();
            view.DataContext = viewModel;
            SectionContent = view;

            Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                x => view.Unloaded += x,
                x => view.Unloaded -= x)
                .Take(1)
                .Subscribe(_ =>
                {
                    busy.Dispose();
                    completed.Dispose();
                });
        }

        void HandleCreatedRepo(LocalRepositoryModel newrepo)
        {
            var msg = String.Format(CultureInfo.CurrentCulture, Constants.Notification_RepoCreated, newrepo.Name, newrepo.CloneUrl);
            msg += " " + String.Format(CultureInfo.CurrentCulture, Constants.Notification_CreateNewProject, newrepo.LocalPath);
            ShowNotification(newrepo, msg);
        }

        private void ShowNotification(LocalRepositoryModel newrepo, string msg)
        {
            var teServices = ServiceProvider.TryGetService<ITeamExplorerServices>();

            teServices.ClearNotifications();
            teServices.ShowMessage(
                msg,
                new RelayCommand(o =>
                {
                    var str = o.ToString();
                    /* the prefix is the action to perform:
                     * u: launch browser with url
                     * c: launch create new project dialog
                     * o: launch open existing project dialog 
                    */
                    var prefix = str.Substring(0, 2);
                    if (prefix == "u:")
                        OpenInBrowser(ServiceProvider.TryGetService<IVisualStudioBrowser>(), new Uri(str.Substring(2)));
                    else if (prefix == "o:")
                    {
                        if (ErrorHandler.Succeeded(ServiceProvider.GetSolution().OpenSolutionViaDlg(str.Substring(2), 1)))
                            ServiceProvider.TryGetService<ITeamExplorer>()?.NavigateToPage(new Guid(TeamExplorerPageIds.Home), null);
                    }
                    else if (prefix == "c:")
                    {
                        var vsGitServices = ServiceProvider.TryGetService<IVSGitServices>();
                        vsGitServices.SetDefaultProjectPath(newrepo.LocalPath);
                        if (ErrorHandler.Succeeded(ServiceProvider.GetSolution().CreateNewProjectViaDlg(null, null, 0)))
                            ServiceProvider.TryGetService<ITeamExplorer>()?.NavigateToPage(new Guid(TeamExplorerPageIds.Home), null);
                    }
                })
            );
        }

        async Task Login()
        {
            await dialogService.ShowLoginDialog();
            loggedIn = await ConnectionManager.IsLoggedIn();
            if (loggedIn)
                ShowPublish();
        }

        bool disposed;
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    disposed = true;
                }
            }
            base.Dispose(disposing);
        }

        string name = String.Empty;
        public string Name
        {
            get { return name; }
            set { name = value; this.RaisePropertyChange(); }
        }

        string provider = String.Empty;
        public string Provider
        {
            get { return provider; }
            set { provider = value; this.RaisePropertyChange(); }
        }

        string description = String.Empty;
        public string Description
        {
            get { return description; }
            set { description = value; this.RaisePropertyChange(); }
        }

        bool showLogin;
        public bool ShowLogin
        {
            get { return showLogin; }
            set { showLogin = value; this.RaisePropertyChange(); }
        }


        bool showSignup;
        public bool ShowSignup
        {
            get { return showSignup; }
            set { showSignup = value; this.RaisePropertyChange(); }
        }

        bool showGetStarted;
        public bool ShowGetStarted
        {
            get { return showGetStarted; }
            set { showGetStarted = value; this.RaisePropertyChange(); }
        }
    }
}
