using System;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using Microsoft.TeamFoundation.MVVM;
using System.Globalization;
using GitHub.Primitives;

namespace GitHub.VisualStudio.TeamExplorer.Sync
{
    public class EnsureLoggedInSection : TeamExplorerSectionBase
    {
        readonly IRepositoryHosts hosts;
        readonly IVSServices vsServices;

        public EnsureLoggedInSection(ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder,
            IConnectionManager cm, IRepositoryHosts hosts, IVSServices vsServices)
            : base(apiFactory, holder, cm)
        {
            IsVisible = false;
            this.hosts = hosts;
            this.vsServices = vsServices;
        }

        public override void Initialize(IServiceProvider serviceProvider)
        {
            base.Initialize(serviceProvider);
            CheckLogin();
        }

        protected override void RepoChanged()
        {
            base.RepoChanged();
            CheckLogin();
        }

        async void CheckLogin()
        {
            // this is not a github repo, or it hasn't been published yet
            if (ActiveRepo == null || ActiveRepoUri == null)
                return;

            vsServices.ClearNotifications();
            var add = HostAddress.Create(ActiveRepoUri);
            bool loggedIn = await connectionManager.IsLoggedIn(hosts, add);
            if (!loggedIn)
            {
                var msg = string.Format(CultureInfo.CurrentUICulture, Resources.NotLoggedInMessage, add.Title, add.ApiUri);
                vsServices.ShowMessage(
                    msg,
                    new RelayCommand(() => StartFlow(UIControllerFlow.Authentication))
                );
            }
        }

        void StartFlow(UIControllerFlow controllerFlow)
        {
            var uiProvider = ServiceProvider.GetExportedValue<IUIProvider>();
            var ret = uiProvider.SetupUI(controllerFlow, null);
            ret.Subscribe(c => { }, CheckLogin);
            uiProvider.RunUI();
        }
    }
}