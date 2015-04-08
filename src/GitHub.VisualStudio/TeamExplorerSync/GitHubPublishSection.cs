using System;
using System.ComponentModel.Composition;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using System.Linq;
using GitHub.Models;
using GitHub.Services;
using GitHub.Info;
using ReactiveUI;
using System.Reactive.Linq;
using GitHub.Extensions;

namespace GitHub.VisualStudio.TeamExplorerHome
{
    [TeamExplorerSection(GitHubPublishSectionId, TeamExplorerPageIds.GitCommits, 10)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubPublishSection : TeamExplorerSectionBase, IGitHubInvitationSection
    {
        public const string GitHubPublishSectionId = "92655B52-360D-4BF5-95C5-D9E9E596AC76";

        readonly IConnectionManager connectionManager;
        readonly Lazy<IVisualStudioBrowser> lazyBrowser;
        IDisposable disposable;

        string description = String.Empty;
        public string Description
        {
            get { return description; }
            set { description = value; this.RaisePropertyChange(); }
        }

        [ImportingConstructor]
        public GitHubPublishSection(IConnectionManager cm, Lazy<IVisualStudioBrowser> browser)
        {
            connectionManager = cm;
            lazyBrowser = browser;
            Title = "GitHub";
            IsVisible = false;
            IsExpanded = true;
            Description = "Powerful collaboration, code review, and code management for open source and private projects.";

            cm.Connections.CollectionChanged += (s,e) => Refresh();
        }

        protected override void Initialize()
        {
            base.Initialize();
            Refresh();
        }

        protected override void ContextChanged(object sender, ContextChangedEventArgs e)
        {
            base.ContextChanged(sender, e);
            Refresh();
        }

        public override void Refresh()
        {
            base.Refresh();

            if (activeRepo != null)
            {
                var repo = Services.GetRepoFromIGit(activeRepo);
                var remote = repo.Network.Remotes.FirstOrDefault(x => x.Name.Equals("origin", StringComparison.Ordinal));
                if (remote == null)
                {
                    IsVisible = true;
                    if (connectionManager.Connections.Count > 0)
                        ShowPublish();
                    else
                        ShowInvitation();
                }
                else
                {
                    IsVisible = false;
                }
            }
        }

        bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (disposable != null)
                        disposable.Dispose();
                }
                disposed = true;
            }
            base.Dispose(disposing);
        }

        public void Connect()
        {
            StartFlow(UIControllerFlow.Authentication);
        }

        public void SignUp()
        {
            OpenInBrowser(lazyBrowser, GitHubUrls.Plans);
        }

        void StartFlow(UIControllerFlow controllerFlow)
        {
            var uiProvider = ServiceProvider.GetExportedValue<IUIProvider>();
            uiProvider.RunUI(controllerFlow, null);
        }

        void ShowInvitation()
        {
            var view = new GitHubInvitationContent();
            SectionContent = view;
            view.DataContext = this;
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
            },
            () =>
            {
                var notificationManager = ServiceProvider.TryGetService<ITeamExplorerNotificationManager>();
                if (notificationManager != null)
                    notificationManager.ShowNotification("Repository published.", NotificationType.Information, NotificationFlags.None, null, default(Guid));
            });
            ui.Start(null);
        }
    }
}