using GitHub.Api;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Controls;
using NullGuard;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using GitHub.Extensions;

namespace GitHub.VisualStudio.TeamExplorer.Connect
{
    public class GitHubConnectSection : TeamExplorerSectionBase, IGitHubConnectSection, INotifyPropertyChanged
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

        internal ITeamExplorerServiceHolder Holder { get { return holder; } }

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
            else if (SectionConnection != connection)
            {
                SectionConnection = connection;
                Repositories = connection.Repositories;
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

            // when we start up the connection list is loaded before we can get notifications so refresh manually
            if (SectionConnection == null && connectionManager.Connections.Count > sectionIndex)
                Refresh(connectionManager.Connections[sectionIndex]);
        }

        void UpdateConnection()
        {
            if (connectionManager.Connections.Count > sectionIndex)
                Refresh(connectionManager.Connections[sectionIndex]);
            else
                Refresh(null);
        }

        void OnPropertyChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsVisible" && IsVisible && View == null)
            {
                View = new GitHubConnectContent();
                View.ViewModel = this;
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
            // this is done here and not via the constructor so nothing gets loaded
            // until we get here
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

        void StartFlow(UIControllerFlow controllerFlow)
        {
            var uiProvider = ServiceProvider.GetExportedValue<IUIProvider>();
            uiProvider.GitServiceProvider = ServiceProvider;
            uiProvider.RunUI(controllerFlow, SectionConnection);
        }

        ObservableCollection<ISimpleRepositoryModel> repositories;
        public ObservableCollection<ISimpleRepositoryModel> Repositories
        {
            [return: AllowNull]
            get { return repositories; }
            set { repositories = value; this.RaisePropertyChange(); }
        }
    }
}
