using GitHub.Api;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Controls;
using System.Collections.Specialized;

namespace GitHub.VisualStudio.TeamExplorerConnect
{
    public class GitHubConnectSection : TeamExplorerSectionBase
    {
        readonly int sectionIndex;

        protected GitHubConnectContent View
        {
            get { return SectionContent as GitHubConnectContent; }
            set { SectionContent = value; }
        }

        protected IConnection SectionConnection { get; set; }

        public GitHubConnectSection(ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder, IConnectionManager manager, int index)
            : base(apiFactory, holder, manager)
        {
            Title = "GitHub";
            IsEnabled = true;
            IsVisible = false;
            IsExpanded = true;

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
                IsVisible = false;
            }
            else if (SectionConnection != connection)
            {
                SectionConnection = connection;
                Title = connection.HostAddress.Title;
                IsVisible = true;
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

        void StartFlow(UIControllerFlow controllerFlow)
        {
            var uiProvider = ServiceProvider.GetExportedValue<IUIProvider>();
            uiProvider.GitServiceProvider = ServiceProvider;
            uiProvider.RunUI(controllerFlow, SectionConnection);
        }
    }
}
