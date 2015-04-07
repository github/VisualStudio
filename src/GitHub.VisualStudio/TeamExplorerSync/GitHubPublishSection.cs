using System;
using System.ComponentModel.Composition;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.Shell;
using System.Linq;
using GitHub.Models;
using GitHub.VisualStudio.UI.Views.Controls;
using GitHub.Services;
using GitHub.Info;

namespace GitHub.VisualStudio.TeamExplorerHome
{
    [TeamExplorerSection(GitHubPublishSectionId, TeamExplorerPageIds.GitCommits, 10)]
    public class GitHubPublishSection : TeamExplorerSectionBase, IGitHubInvitationSection
    {
        public const string GitHubPublishSectionId = "92655B52-360D-4BF5-95C5-D9E9E596AC76";

        readonly IConnectionManager connectionManager;
        readonly Lazy<IVisualStudioBrowser> lazyBrowser;

        string description = String.Empty;
        public string Description
        {
            get { return description; }
            set { description = value; this.RaisePropertyChange(); }
        }

        [ImportingConstructor]
        public GitHubPublishSection([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            IConnectionManager cm, Lazy<IVisualStudioBrowser> browser)
            :  base(serviceProvider)
        {
            connectionManager = cm;
            lazyBrowser = browser;
            Title = "GitHub";
            IsVisible = false;
            IsExpanded = true;
            Description = "Powerful collaboration, code review, and code management for open source and private projects.";
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
            view.DataContext = this;
            SectionContent = view;
        }

        void ShowPublish()
        {
            var view = new RepositoryPublishControl();
            view.DataContext = this;
            SectionContent = view;
        }
    }
}