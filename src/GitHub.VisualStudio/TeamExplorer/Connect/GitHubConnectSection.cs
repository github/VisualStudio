using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using GitHub.Api;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.MVVM;
using Microsoft.VisualStudio;
using NullGuard;
using ReactiveUI;

namespace GitHub.VisualStudio.TeamExplorer.Connect
{
    public class GitHubConnectSection : TeamExplorerSectionBase, IGitHubConnectSection
    {
        readonly int sectionIndex;
        bool isCloning;
        bool isCreating;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        SectionStateTracker sectionTracker;

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

        IReactiveDerivedList<ISimpleRepositoryModel> repositories;
        [AllowNull]
        public IReactiveDerivedList<ISimpleRepositoryModel> Repositories
        {
            [return:AllowNull]
            get { return repositories; }
            set { repositories = value; this.RaisePropertyChange(); }
        }

        ISimpleRepositoryModel selectedRepository;
        [AllowNull]
        public ISimpleRepositoryModel SelectedRepository
        {
            [return: AllowNull]
            get { return selectedRepository; }
            set { selectedRepository = value; this.RaisePropertyChange(); }
        }

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

        void RefreshConnections(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (connectionManager.Connections.Count > sectionIndex)
                        Refresh(connectionManager.Connections[sectionIndex]);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Refresh(connectionManager.Connections.Count <= sectionIndex
                        ? null
                        : connectionManager.Connections[sectionIndex]);
                    break;
            }
        }

        protected void Refresh(IConnection connection)
        {
            if (connection == null)
            {
                LoggedIn = false;
                IsVisible = false;
                SectionConnection = null;
                if (Repositories != null)
                    Repositories.CollectionChanged -= UpdateRepositoryList;
                Repositories = null;

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
            else
            {
                if (connection != SectionConnection)
                {
                    SectionConnection = connection;
                    Repositories = SectionConnection.Repositories.CreateDerivedCollection(x => x,
                                        orderer: OrderedComparer<ISimpleRepositoryModel>.OrderBy(x => x.Name).Compare);
                    Repositories.CollectionChanged += UpdateRepositoryList;
                    Title = connection.HostAddress.Title;
                    IsVisible = true;
                    LoggedIn = true;
                    if (ServiceProvider != null)
                        RefreshRepositories();
                }
            }
        }

        public override void Refresh()
        {
            UpdateConnection();
            base.Refresh();
        }

        public override void Initialize(IServiceProvider serviceProvider)
        {
            base.Initialize(serviceProvider);
            UpdateConnection();

            // watch for new repos added to the local repo list
            var section = ServiceProvider.GetSection(TeamExplorerConnectionsSectionId);
            if (section != null)
                sectionTracker = new SectionStateTracker(section, RefreshRepositories);
        }

        void UpdateConnection()
        {
            Refresh(connectionManager.Connections.Count > sectionIndex
                ? connectionManager.Connections[sectionIndex]
                : SectionConnection);
        }

        void OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsVisible" && IsVisible && View == null)
                View = new GitHubConnectContent { DataContext = this };
        }

        async void UpdateRepositoryList(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // if we're cloning or creating, only one repo will be added to the list
                // so we can handle just one new entry separately
                if (isCloning || isCreating)
                {
                    var newrepo = e.NewItems.Cast<ISimpleRepositoryModel>().First();

                    SelectedRepository = newrepo;
                    if (isCreating)
                        HandleCreatedRepo(newrepo);
                    else
                        HandleClonedRepo(newrepo);

                    isCreating = isCloning = false;
                    var repo = await ApiFactory.Create(newrepo.CloneUrl).GetRepository();
                    newrepo.SetIcon(repo.Private, repo.Fork);
                }
                // looks like it's just a refresh with new stuff on the list, update the icons
                else
                {
                    e.NewItems
                        .Cast<ISimpleRepositoryModel>()
                        .ForEach(async r =>
                    {
                        if (Equals(Holder.ActiveRepo, r))
                            SelectedRepository = r;
                        var repo = await ApiFactory.Create(r.CloneUrl).GetRepository();
                        r.SetIcon(repo.Private, repo.Fork);
                    });
                }
            }
        }

        void HandleCreatedRepo(ISimpleRepositoryModel newrepo)
        {
            var msg = string.Format(CultureInfo.CurrentUICulture, Constants.Notification_RepoCreated, newrepo.Name, newrepo.CloneUrl);
            msg += " " + string.Format(CultureInfo.CurrentUICulture, Constants.Notification_CreateNewProject, newrepo.LocalPath);
            ShowNotification(newrepo, msg);
        }

        void HandleClonedRepo(ISimpleRepositoryModel newrepo)
        {
            var msg = string.Format(CultureInfo.CurrentUICulture, Constants.Notification_RepoCloned, newrepo.Name, newrepo.CloneUrl);
            if (newrepo.HasCommits() && newrepo.MightContainSolution())
                msg += " " + string.Format(CultureInfo.CurrentUICulture, Constants.Notification_OpenProject, newrepo.LocalPath);
            else
                msg += " " + string.Format(CultureInfo.CurrentUICulture, Constants.Notification_CreateNewProject, newrepo.LocalPath);
            ShowNotification(newrepo, msg);
        }

        void ShowNotification(ISimpleRepositoryModel newrepo, string msg)
        {
            var vsservices = ServiceProvider.GetExportedValue<IVSServices>();
            vsservices.ClearNotifications();
            vsservices.ShowMessage(
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
                        vsservices.SetDefaultProjectPath(newrepo.LocalPath);
                        if (ErrorHandler.Succeeded(ServiceProvider.GetSolution().CreateNewProjectViaDlg(null, null, 0)))
                            ServiceProvider.TryGetService<ITeamExplorer>()?.NavigateToPage(new Guid(TeamExplorerPageIds.Home), null);
                    }
                })
            );
#if DEBUG
            VsOutputLogger.WriteLine(String.Format(CultureInfo.InvariantCulture, "{0} Notification", DateTime.Now));
#endif
        }

        void RefreshRepositories()
        {
            connectionManager.RefreshRepositories();
            RaisePropertyChanged("Repositories"); // trigger a re-check of the visibility of the listview based on item count
        }

        public void DoCreate()
        {
            StartFlow(UIControllerFlow.Create);
        }

        public void DoClone()
        {
            StartFlow(UIControllerFlow.Clone);
        }

        public void SignOut()
        {
            SectionConnection.Logout();
        }

        public void Login()
        {
            StartFlow(UIControllerFlow.Authentication);
        }

        public bool OpenRepository()
        {
            var old = Repositories.FirstOrDefault(x => x.Equals(Holder.ActiveRepo));
            // open the solution selection dialog when the user wants to switch to a different repo
            // since there's no other way of changing the source control context in VS
            if (!Equals(SelectedRepository, old))
            {
                if (ErrorHandler.Succeeded(ServiceProvider.GetSolution().OpenSolutionViaDlg(SelectedRepository.LocalPath, 1)))
                {
                    ServiceProvider.TryGetService<ITeamExplorer>()?.NavigateToPage(new Guid(TeamExplorerPageIds.Home), null);
                    return true;
                }
                else
                {
                    SelectedRepository = old;
                    return false;
                }
            }
            return false;
        }

        void StartFlow(UIControllerFlow controllerFlow)
        {
            var uiProvider = ServiceProvider.GetExportedValue<IUIProvider>();
            uiProvider.GitServiceProvider = ServiceProvider;
            var ret = uiProvider.SetupUI(controllerFlow, SectionConnection);
            ret.Subscribe(c =>
            {
                if (c.IsViewType(UIViewType.Clone))
                    isCloning = true;
                else if (c.IsViewType(UIViewType.Create))
                    isCreating = true;
            });
            uiProvider.RunUI();
        }

        bool disposed;
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    connectionManager.Connections.CollectionChanged -= RefreshConnections;
                    if (Repositories != null)
                        Repositories.CollectionChanged -= UpdateRepositoryList;
                    disposed = true;
                }
            }
            base.Dispose(disposing);
        }


        class SectionStateTracker
        {
            enum SectionState
            {
                Idle,
                Busy,
                Refreshing
            }

            readonly Stateless.StateMachine<SectionState, string> machine;
            readonly ITeamExplorerSection section;

            public SectionStateTracker(ITeamExplorerSection section, Action onRefreshed)
            {
                this.section = section;
                machine = new Stateless.StateMachine<SectionState, string>(SectionState.Idle);
                machine.Configure(SectionState.Idle)
                    .PermitIf("IsBusy", SectionState.Busy, () => this.section.IsBusy)
                    .IgnoreIf("IsBusy", () => !this.section.IsBusy);
                machine.Configure(SectionState.Busy)
                    .Permit("Title", SectionState.Refreshing)
                    .PermitIf("IsBusy", SectionState.Idle, () => !this.section.IsBusy)
                    .IgnoreIf("IsBusy", () => this.section.IsBusy);
                machine.Configure(SectionState.Refreshing)
                    .Ignore("Title")
                    .PermitIf("IsBusy", SectionState.Idle, () => !this.section.IsBusy)
                    .IgnoreIf("IsBusy", () => this.section.IsBusy)
                    .OnExit(onRefreshed);

                section.PropertyChanged += TrackState;
            }
#if DEBUG
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
#endif
            void TrackState(object sender, PropertyChangedEventArgs e)
            {
                if (machine.PermittedTriggers.Contains(e.PropertyName))
                {
#if DEBUG
                    VsOutputLogger.WriteLine(String.Format(CultureInfo.InvariantCulture, "{3} {0} title:{1} busy:{2}", e.PropertyName, ((ITeamExplorerSection)sender).Title, ((ITeamExplorerSection)sender).IsBusy, DateTime.Now));
#endif
                    machine.Fire(e.PropertyName);
                }
            }
        }
    }
}
