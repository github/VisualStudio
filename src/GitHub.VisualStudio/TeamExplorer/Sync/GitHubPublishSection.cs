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
using System.Reactive.Linq;
using GitHub.Extensions;
using GitHub.Api;
using System.Reactive.Disposables;
using System.Windows.Controls;

namespace GitHub.VisualStudio.TeamExplorer.Sync
{
    [TeamExplorerSection(GitHubPublishSectionId, TeamExplorerPageIds.GitCommits, 10)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubPublishSection : TeamExplorerSectionBase, IGitHubInvitationSection
    {
        public const string GitHubPublishSectionId = "92655B52-360D-4BF5-95C5-D9E9E596AC76";

        readonly Lazy<IVisualStudioBrowser> lazyBrowser;
        readonly IRepositoryHosts hosts;
        readonly CompositeDisposable disposables = new CompositeDisposable();
        bool loggedIn;
        readonly UserControl view;

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
            view = new GitHubInvitationContent();
            SectionContent = view;
            view.DataContext = this;
        }

        async void Setup()
        {
            if (ActiveRepo != null && ActiveRepoUri == null)
            {
                IsVisible = true;
                loggedIn = await connectionManager.IsLoggedIn(hosts);
                ShowGetStarted = true;
                ShowLogin = !loggedIn;
                ShowSignup = !loggedIn;
            }
            else
                IsVisible = false;
        }

        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            base.Initialize(sender, e);
            Setup();
        }

        protected override void RepoChanged()
        {
            base.RepoChanged();
            Setup();
        }

        public async void Connect()
        {
            loggedIn = await connectionManager.IsLoggedIn(hosts);
            // we run the login on a separate UI flow because the login
            // dialog is a modal dialog while the publish dialog is inlined in Team Explorer
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
            // set the loading indicator while we prep the form
            IsBusy = true;

            var uiProvider = ServiceProvider.GetExportedValue<IUIProvider>();
            var factory = uiProvider.GetService<IExportFactoryProvider>();
            var uiflow = factory.UIControllerFactory.CreateExport();
            disposables.Add(uiflow);
            var ui = uiflow.Value;
            var creation = ui.SelectFlow(UIControllerFlow.Publish);
            var busyTracker = new SerialDisposable();
            creation.Subscribe(c =>
            {
                SectionContent = c;
                c.DataContext = this;

                var v = (IView)c;
                busyTracker.Disposable = v.IsBusy.Subscribe(x => IsBusy = x);
                disposables.Add(v.Error.Subscribe(x =>
                {
                    var vsServices = ServiceProvider.GetExportedValue<IVSServices>();
                    vsServices.ShowError(x as string);
                }));
            },
            () =>
            {
                var vsServices = ServiceProvider.GetExportedValue<IVSServices>();
                vsServices.ShowMessage("Repository published successfully.");

                var v = SectionContent as IView;
                // the IsPublishing flag takes way too long to fire, don't wait for it, we're done
                if (IsBusy)
                {
                    // we only want to dispose things when all the events are processed (IsPublishing is the last one)
                    busyTracker.Disposable = v.IsBusy.Subscribe(_ =>
                    {
                        disposables.Clear();
                        busyTracker.Dispose();
                    });
                    IsBusy = false;
                }
                else
                {
                    busyTracker.Dispose();
                    disposables.Clear();
                }
                SectionContent = view;
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
                    disposables.Dispose();
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