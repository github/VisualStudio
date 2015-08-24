using GitHub.Api;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Controls;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using GitHub.Extensions;
using NullGuard;
using Microsoft.VisualStudio;
using System;

namespace GitHub.VisualStudio.TeamExplorer.Connect
{
    public class GitHubConnectSection : TeamExplorerSectionBase, IGitHubConnectSection
    {
        readonly int sectionIndex;

        protected GitHubConnectContent View
        {
            get { return SectionContent as GitHubConnectContent; }
            set { SectionContent = value; }
        }

        protected IConnection SectionConnection { get; set; }

        bool loggedIn;
        bool LoggedIn
        {
            get { return loggedIn; }
            set {
                loggedIn = ShowLogout = value;
                ShowLogin = !value;
            }
        }

        bool showLogin;
        public bool ShowLogin
        {
            get { return showLogin; }
            set { showLogin = value; this.RaisePropertyChange(); }
        }

        bool showLogout;
        public bool ShowLogout
        {
            get { return showLogout; }
            set { showLogout = value; this.RaisePropertyChange(); }
        }

        ObservableCollection<ISimpleRepositoryModel> repositories;
        public ObservableCollection<ISimpleRepositoryModel> Repositories
        {
            [return:AllowNull]
            get { return repositories; }
            set { repositories = value; this.RaisePropertyChange(); }
        }

        ISimpleRepositoryModel selectedRepository;

        [AllowNull]
        public ISimpleRepositoryModel SelectedRepository {
            [return: AllowNull] get { return selectedRepository; }
            set { selectedRepository = value; this.RaisePropertyChange(); } }

        internal ITeamExplorerServiceHolder Holder => holder;
        bool cloning;

        public GitHubConnectSection(ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder, IConnectionManager manager, int index)
            : base(apiFactory, holder, manager)
        {
            Title = "GitHub";
            IsEnabled = true;
            IsVisible = false;
            IsExpanded = true;
            LoggedIn = false;

            sectionIndex = index;

            connectionManager.Connections.CollectionChanged += RefreshConnections;
            PropertyChanged += OnPropertyChange;
            UpdateConnection();
        }

        private void RefreshConnections(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (connectionManager.Connections.Count > sectionIndex)
                        Refresh(connectionManager.Connections[sectionIndex]);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (connectionManager.Connections.Count <= sectionIndex)
                        Refresh(null);
                    break;
            }
        }

        protected void Refresh(IConnection connection)
        {
            if (connection == null)
            {
                LoggedIn = false;
                IsVisible = false;
                if (sectionIndex == 0 && ServiceProvider != null)
                {
                    var section = ServiceProvider?.GetSection(TeamExplorerInvitationBase.TeamExplorerInvitationSectionGuid);
                    IsVisible = !(section?.IsVisible ?? true); // only show this when the invitation section is hidden. When in doubt, don't show it.
                    if (section != null)
                        section.PropertyChanged += (s, p) =>
                        {
                            if (p.PropertyName == "IsVisible")
                                IsVisible = LoggedIn || !((ITeamExplorerSection)s).IsVisible;
                        };
                }
            }
            else
            {
                SectionConnection = connection;
                Repositories = SectionConnection.Repositories;
                SelectedRepository = Repositories.FirstOrDefault(x => String.Equals(x.LocalPath, Holder.ActiveRepo?.RepositoryPath, StringComparison.CurrentCultureIgnoreCase));
                Repositories.CollectionChanged += UpdateIcons;
                var section = ServiceProvider.GetSection(TeamExplorerConnectionsSectionId);
                if (section != null)
                    section.PropertyChanged += UpdateRepositoryList;

                Title = connection.HostAddress.Title;
                IsVisible = true;
                LoggedIn = true;
            }
        }

        public override void Refresh()
        {
            UpdateConnection();
            base.Refresh();
        }

        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            base.Initialize(sender, e);
            if (sectionIndex == 0)
                connectionManager.RefreshRepositories(ServiceProvider.GetExportedValue<IVSServices>());
            UpdateConnection();
        }

        void UpdateConnection()
        {
            if (connectionManager.Connections.Count > sectionIndex)
                Refresh(connectionManager.Connections[sectionIndex]);
            else
                Refresh(SectionConnection);
        }

        void OnPropertyChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsVisible" && IsVisible && View == null)
                View = new GitHubConnectContent { DataContext = this };
        }

        void UpdateIcons(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                e.NewItems
                    .Cast<ISimpleRepositoryModel>()
                    .Select(async r =>
                {
                    if (String.Equals(Holder.ActiveRepo?.RepositoryPath, r.LocalPath, StringComparison.CurrentCultureIgnoreCase))
                        SelectedRepository = r;
                    var repo = await ApiFactory.Create(r.CloneUrl).GetRepository();
                    r.SetIcon(repo.Private, repo.Fork);
                }).ToList();
            }
        }

        void UpdateRepositoryList(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (cloning && e.PropertyName == "IsBusy" && !((ITeamExplorerSection)sender).IsBusy)
            {
                Repositories.CollectionChanged += HandleClonedRepo;
                connectionManager.RefreshRepositories(ServiceProvider.GetExportedValue<IVSServices>());
                Repositories.CollectionChanged -= HandleClonedRepo;
                cloning = false;
            }
        }


        public void DoCreate()
        {
            // this is done here and not via the constructor so nothing gets loaded
            // until we get here
            StartFlow(UIControllerFlow.Create);
        }

        public void DoClone()
        {
            cloning = true;
            // this is done here and not via the constructor so nothing gets loaded
            // until we get here
            StartFlow(UIControllerFlow.Clone);
        }

        void HandleClonedRepo(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var newrepo = e.NewItems.Cast<ISimpleRepositoryModel>().FirstOrDefault();
                if (newrepo != null)
                {
                    SelectedRepository = newrepo;
                    OpenRepository();
                }
            }
        }

        public void SignOut()
        {
            SectionConnection.Logout();
        }

        public void Login()
        {
            StartFlow(UIControllerFlow.Authentication);
        }

        public void OpenRepository()
        {
            var old = Repositories.FirstOrDefault(x => String.Equals(Holder.ActiveRepo?.RepositoryPath, x.LocalPath, StringComparison.CurrentCultureIgnoreCase));
            // open the solution selection dialog when the user wants to switch to a different repo
            // since there's no other way of changing the source control context in VS
            if (SelectedRepository != old)
            {
                if (ErrorHandler.Failed(ServiceProvider.GetSolution().OpenSolutionViaDlg(SelectedRepository.LocalPath, 1)))
                    SelectedRepository = old;
                else
                {
                    var service = ServiceProvider.TryGetService<ITeamExplorer>();
                    if (service?.NavigateToPage(new Guid(TeamExplorerPageIds.Home), null) == null)
                        SelectedRepository = old;
                }
            }
        }

        void StartFlow(UIControllerFlow controllerFlow)
        {
            var uiProvider = ServiceProvider.GetExportedValue<IUIProvider>();
            uiProvider.GitServiceProvider = ServiceProvider;
            uiProvider.RunUI(controllerFlow, SectionConnection);
        }

        bool disposed;
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    if (Repositories != null)
                        Repositories.CollectionChanged -= UpdateIcons;
                    disposed = true;
                }
            }
            base.Dispose(disposing);
        }
    }
}
