using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.UI;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Controls;
using System;
using System.Collections.Specialized;

namespace GitHub.VisualStudio.TeamExplorerConnect
{
    public class GitHubConnectSection : TeamExplorerSectionBase
    {
        IServiceProvider gitServiceProvider;
        readonly IConnectionManager connectionManager;
        readonly int sectionIndex;

        protected GitHubConnectContent View
        {
            get { return SectionContent as GitHubConnectContent; }
            set { SectionContent = value; }
        }

        protected IConnection SectionConnection { get; set; }

        public GitHubConnectSection(IConnectionManager manager, int index)
        {
            Title = "GitHub";
            IsEnabled = true;
            IsVisible = false;
            IsExpanded = false;

            connectionManager = manager;
            sectionIndex = index;

            connectionManager.Connections.CollectionChanged += RefreshConnections;
            PropertyChanged += OnPropertyChange;

            Refresh();
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
                IsExpanded = false;
            }
            else if (SectionConnection != connection)
            {
                SectionConnection = connection;
                // TODO: set title according to type of host
                IsVisible = true;
                IsExpanded = true;
            }
        }

        public override void Refresh()
        {
            if (connectionManager.Connections.Count > sectionIndex)
                Refresh(connectionManager.Connections[sectionIndex]);
            else
                Refresh(null);
            base.Refresh();
        }

        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            base.Initialize(sender, e);

            gitServiceProvider = e.ServiceProvider;

            // when we start up the connection list is loaded before we can get notifications so refresh manually
            if (SectionConnection == null && connectionManager.Connections.Count > sectionIndex)
                Refresh(connectionManager.Connections[sectionIndex]);
        }

        private void OnPropertyChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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

        void StartFlow(UIControllerFlow controllerFlow)
        {
            var uiProvider = ServiceProvider.GetExportedValue<IUIProvider>();
            uiProvider.AddService(typeof(IConnection), SectionConnection);

            uiProvider.GitServiceProvider = gitServiceProvider;
            var factory = uiProvider.GetService<IExportFactoryProvider>();
            var uiControllerExport = factory.UIControllerFactory.CreateExport();
            var uiController = uiControllerExport.Value;
            var creation = uiController.SelectFlow(controllerFlow, SectionConnection);
            var windowController = new WindowController(creation);
            creation.Subscribe(_ => { }, _ =>
            {
                windowController.Close();
                uiControllerExport.Dispose();
                uiProvider.RemoveService(typeof(IConnection));
            });
            windowController.Show();

            uiController.Start();
        }
    }
}
