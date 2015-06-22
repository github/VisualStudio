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

namespace GitHub.VisualStudio.TeamExplorerSync
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
            Title = "Publish to GitHub";
            Name = "GitHub";
            Provider = "GitHub, Inc";
            Description = "Powerful collaboration, code review, and code management for open source and private projects.";
            CanSignUp = false;
            CanConnect = true;
            IsVisible = true;
            IsExpanded = true;
            var view = new GitHubInvitationContent();
            SectionContent = view;
            view.DataContext = this;
        }

        public async override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            loggedIn = await connectionManager.IsLoggedIn(hosts);

            CanSignUp = !loggedIn;
            base.Initialize(sender, e);
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
            ret.Subscribe((c) => { }, () =>
            {
                Connect();
            });
            uiProvider.RunUI();
        }

        void ShowPublish()
        {
            var uiProvider = ServiceProvider.GetExportedValue<IUIProvider>();
            var factory = uiProvider.GetService<IExportFactoryProvider>();
            var uiflow = factory.UIControllerFactory.CreateExport();
            disposable = uiflow;
            var ui = uiflow.Value;
            var creation = ui.SelectFlow(UIControllerFlow.Publish);
            creation
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe((c) =>
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

        bool canSignUp;
        public bool CanSignUp
        {
            get { return canSignUp; }
            set { canSignUp = value; this.RaisePropertyChange(); }
        }

        bool canConnect;
        public bool CanConnect
        {
            get { return canConnect; }
            set { canConnect = value; this.RaisePropertyChange(); }
        }
    }
}