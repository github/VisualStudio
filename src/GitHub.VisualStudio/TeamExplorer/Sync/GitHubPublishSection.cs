using System;
using System.ComponentModel.Composition;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Controls;
using GitHub.Models;
using GitHub.Services;
using GitHub.Info;
using ReactiveUI;
using System.Reactive.Linq;
using GitHub.Extensions;
using GitHub.Api;
using GitHub.VisualStudio.TeamExplorer;

namespace GitHub.VisualStudio.TeamExplorer.Sync
{
    [TeamExplorerSection(GitHubPublishSectionId, TeamExplorerPageIds.GitCommits, 10)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubPublishSection : TeamExplorerSectionBase, IGitHubInvitationSection
    {
        public const string GitHubPublishSectionId = "92655B52-360D-4BF5-95C5-D9E9E596AC76";

        readonly Lazy<IVisualStudioBrowser> lazyBrowser;
        readonly IRepositoryHosts hosts;
        IDisposable disposable;
        bool loggedIn;

        [ImportingConstructor]
        public GitHubPublishSection(ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder,
            IConnectionManager cm, Lazy<IVisualStudioBrowser> browser,
            IRepositoryHosts hosts)
            : base(apiFactory, holder, cm)
        {

            lazyBrowser = browser;
            this.hosts = hosts;
            Title = Resources.GitHubPublishSectionTitle;
            Name = "GitHub";
            Provider = "GitHub, Inc";
            Description = Resources.BlurbText;
            ShowLogin = false;
            ShowSignup = false;
            ShowGetStarted = false;
            IsVisible = false;
            IsExpanded = true;
            var view = new GitHubInvitationContent();
            SectionContent = view;
            view.DataContext = this;
        }

        async void RTMSetup()
        {
            if (ActiveRepo != null && ActiveRepoUri == null)
            {
                IsVisible = true;
                ShowGetStarted = true;
                loggedIn = await connectionManager.IsLoggedIn(hosts);
                ShowLogin = !loggedIn;
                ShowSignup = !loggedIn;
            }
            else
                IsVisible = false;
        }

        async void PreRTMSetup()
        {
            if (ActiveRepo != null && ActiveRepoUri == null)
            {
                IsVisible = true;
                loggedIn = await connectionManager.IsLoggedIn(hosts);
                if (loggedIn)
                    ShowPublish();
                else
                {
                    ShowGetStarted = true;
                    ShowSignup = true;
                }
            }
            else
                IsVisible = false;
        }

        public override void Initialize(IServiceProvider serviceProvider)
        {
            base.Initialize(serviceProvider);
            RTMSetup();
        }

        protected override void RepoChanged()
        {
            base.RepoChanged();
            RTMSetup();
        }

        public async void Connect()
        {
            loggedIn = await connectionManager.IsLoggedIn(hosts);
            if (loggedIn)
                ShowPublish();
            else
                Login();
        }

        public void SignUp()
        {
            OpenInBrowser(lazyBrowser, GitHubUrls.Plans);
        }

        public void Login()
        {
            StartFlow(UIControllerFlow.Authentication);
        }

        void StartFlow(UIControllerFlow controllerFlow)
        {
            var uiProvider = ServiceProvider.GetExportedValue<IUIProvider>();
            var ret = uiProvider.SetupUI(controllerFlow, null);
            ret.Subscribe((c) => { }, async () =>
            {
                loggedIn = await connectionManager.IsLoggedIn(hosts);
                if (loggedIn)
                    ShowPublish();
            });
            uiProvider.RunUI();
        }

        void ShowPublish()
        {
            IsBusy = true;
            var uiProvider = ServiceProvider.GetExportedValue<IUIProvider>();
            var factory = uiProvider.GetService<IExportFactoryProvider>();
            var uiflow = factory.UIControllerFactory.CreateExport();
            disposable = uiflow;
            var ui = uiflow.Value;
            var creation = ui.SelectFlow(UIControllerFlow.Publish);
            creation.Subscribe(c =>
            {
                SectionContent = c;
                c.DataContext = this;
                ((IView)c).IsBusy.Subscribe(x => IsBusy = x);
            });
            ui.Start(null);
        }

        bool disposed;
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    if (disposable != null)
                        disposable.Dispose();
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