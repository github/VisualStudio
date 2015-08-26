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
using System.Windows.Data;
using System.ComponentModel;

namespace GitHub.VisualStudio.TeamExplorer.Connect
{
    public class GitHubConnectSection : TeamExplorerSectionBase, IGitHubConnectSection
    {
        readonly int sectionIndex;
        bool isCloning;
        bool isCreating;
        bool runCreateNewProject;
        bool runOpenSolution;

        protected GitHubConnectContent View
        {
            get { return SectionContent as GitHubConnectContent; }
            set { SectionContent = value; }
        }

        [AllowNull]
        public IConnection SectionConnection { [return:AllowNull] get; set; }

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
                    var section = ServiceProvider.GetSection(TeamExplorerInvitationBase.TeamExplorerInvitationSectionGuid);
                    IsVisible = !(section?.IsVisible ?? true); // only show this when the invitation section is hidden. When in doubt, don't show it.
                    if (section != null)
                        section.PropertyChanged += (s, p) =>
                        {
                            if (p.PropertyName == "IsVisible")
                                IsVisible = LoggedIn || !((ITeamExplorerSection)s).IsVisible;
                        };
                }
            }
            else if (connection != SectionConnection)
            {
                SectionConnection = connection;
                Repositories = SectionConnection.Repositories;
                Title = connection.HostAddress.Title;
                IsVisible = true;
                LoggedIn = true;

                // Sort the repo list by name. Only set it once, we don't sort this list
                // anywhere else so no need to reset it every time
                var view = CollectionViewSource.GetDefaultView(Repositories);
                if (view.SortDescriptions.Count == 0)
                {
                    view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                    view.Refresh();
                }

                SelectedRepository = Repositories.FirstOrDefault(x => String.Equals(x.LocalPath, Holder.ActiveRepo?.RepositoryPath, StringComparison.CurrentCultureIgnoreCase));
                Repositories.CollectionChanged += UpdateIcons;
            }
        }

        public override void Refresh()
        {
            UpdateConnection();
            if (sectionIndex == 0)
                connectionManager.RefreshRepositories(ServiceProvider.GetExportedValue<IVSServices>());
            base.Refresh();
        }

        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            base.Initialize(sender, e);
            UpdateConnection();

            // the first time we run, refresh the repositories list for all the connections
            if (sectionIndex == 0)
                connectionManager.RefreshRepositories(ServiceProvider.GetExportedValue<IVSServices>());

            // watch for new repos added to the local repo list
            var section = ServiceProvider.GetSection(TeamExplorerConnectionsSectionId);
            if (section != null)
                section.PropertyChanged += UpdateRepositoryList;

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

        void UpdateRepositoryList(object sender, PropertyChangedEventArgs e)
        {
            if ((isCloning || isCreating) && e.PropertyName == "IsBusy" && !((ITeamExplorerSection)sender).IsBusy)
            {
                var vsservices = ServiceProvider.GetExportedValue<IVSServices>();

                if (isCreating)
                {
                    vsservices.ClearNotifications();
                    vsservices.ShowMessage("The repository has been successfully created.");
                }

                // this property change might trigger multiple times and we only want
                // the first time, so switch out flags to keep this code from running more than once per action
                runOpenSolution = isCloning;
                runCreateNewProject = isCreating;
                isCloning = isCreating = false;

                Repositories.CollectionChanged += HandleNewRepo;
                connectionManager.RefreshRepositories(vsservices);
                Repositories.CollectionChanged -= HandleNewRepo;
            }
        }


        public void DoCreate()
        {
            isCreating = true;
            StartFlow(UIControllerFlow.Create);
        }

        public void DoClone()
        {
            isCloning = true;
            StartFlow(UIControllerFlow.Clone);
        }

        void HandleNewRepo(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var newrepo = e.NewItems.Cast<ISimpleRepositoryModel>().FirstOrDefault();
                if (newrepo != null)
                {
                    SelectedRepository = newrepo;
                    if (runOpenSolution)
                        OpenRepository();
                    else if (runCreateNewProject)
                        CreateNewProject();
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
                if (ErrorHandler.Succeeded(ServiceProvider.GetSolution().OpenSolutionViaDlg(SelectedRepository.LocalPath, 1)))
                    ServiceProvider.TryGetService<ITeamExplorer>()?.NavigateToPage(new Guid(TeamExplorerPageIds.Home), null);
                else
                    SelectedRepository = old;
            }
        }

        public void CreateNewProject()
        {
            var old = Repositories.FirstOrDefault(x => String.Equals(Holder.ActiveRepo?.RepositoryPath, x.LocalPath, StringComparison.CurrentCultureIgnoreCase));
            // open the solution selection dialog when the user wants to switch to a different repo
            // since there's no other way of changing the source control context in VS
            if (SelectedRepository != old)
            {
                var vsservices = ServiceProvider.GetExportedValue<IVSServices>();
                var oldpath = vsservices.SetDefaultProjectPath(SelectedRepository.LocalPath);
                if (ErrorHandler.Succeeded(ServiceProvider.GetSolution().CreateNewProjectViaDlg("", "", 0)))
                    ServiceProvider.TryGetService<ITeamExplorer>()?.NavigateToPage(new Guid(TeamExplorerPageIds.Home), null);
                else
                {
                    SelectedRepository = old;
                    vsservices.SetDefaultProjectPath(oldpath);
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
