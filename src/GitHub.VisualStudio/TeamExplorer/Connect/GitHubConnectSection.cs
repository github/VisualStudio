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
                    if (connectionManager.Connections.Count <= sectionIndex)
                        Refresh(null);
                    else
                        Refresh(connectionManager.Connections[sectionIndex]);
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

        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            base.Initialize(sender, e);
            UpdateConnection();

            // watch for new repos added to the local repo list
            var section = ServiceProvider.GetSection(TeamExplorerConnectionsSectionId);
            if (section != null)
                sectionTracker = new SectionStateTracker(section, RefreshRepositories);
        }

        void UpdateConnection()
        {
            if (connectionManager.Connections.Count > sectionIndex)
                Refresh(connectionManager.Connections[sectionIndex]);
            else
                Refresh(SectionConnection);
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
                    // if we've cloned a repo but the user didn't open a project in it,
                    // then update the newly-cloned repo icon because we're not going to
                    // switch to the TE home page
                    if (isCloning && !OpenRepository())
                    {
                        var repo = await ApiFactory.Create(newrepo.CloneUrl).GetRepository();
                        newrepo.SetIcon(repo.Private, repo.Fork);
                    }
                }
                // looks like it's just a refresh with new stuff on the list, update the icons
                else
                {
                    e.NewItems
                        .Cast<ISimpleRepositoryModel>()
                        .ForEach(async r =>
                    {
                        if (String.Equals(Holder.ActiveRepo?.RepositoryPath, r.LocalPath, StringComparison.CurrentCultureIgnoreCase))
                            SelectedRepository = r;
                        var repo = await ApiFactory.Create(r.CloneUrl).GetRepository();
                        r.SetIcon(repo.Private, repo.Fork);
                    });
                }
            }
        }

        void RefreshRepositories()
        {
            connectionManager.RefreshRepositories();
            RaisePropertyChanged("Repositories"); // trigger a re-check of the visibility of the listview based on item count
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
            var old = Repositories.FirstOrDefault(x => String.Equals(Holder.ActiveRepo?.RepositoryPath, x.LocalPath, StringComparison.CurrentCultureIgnoreCase));
            // open the solution selection dialog when the user wants to switch to a different repo
            // since there's no other way of changing the source control context in VS
            if (SelectedRepository != old)
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

            void TrackState(object sender, PropertyChangedEventArgs e)
            {
                if (machine.PermittedTriggers.Contains(e.PropertyName))
                    machine.Fire(e.PropertyName);
            }
        }
    }
}
