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
using System.ComponentModel;
using GitHub.VisualStudio.UI.Views.Controls;

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
            Title = "Publish to GitHub";
            IsVisible = false;
            IsExpanded = true;
            Description = "Powerful collaboration, code review, and code management for open source and private projects.";

            cm.Connections.CollectionChanged += (s,e) => Refresh();
        }

        protected override void RepoChanged()
        {
            if (ActiveRepo != null && ActiveRepoUri == null)
            {
                // not published yet
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
            base.RepoChanged();
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
            var ret = uiProvider.SetupUI(controllerFlow, null);
            ret.Subscribe((c) => { }, () =>
            {
                Initialize();
            });
            uiProvider.RunUI();
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
                    ((IView)c).IsBusy.Subscribe(x => IsBusy = x);
                });
            ui.Start(null);
        }
    }
}