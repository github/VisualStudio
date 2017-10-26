using System;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using System.Globalization;
using GitHub.Primitives;
using System.Threading.Tasks;
using GitHub.VisualStudio.UI;

namespace GitHub.VisualStudio.TeamExplorer.Sync
{
    public class EnsureLoggedInSection : TeamExplorerSectionBase
    {
        readonly Lazy<IRepositoryHosts> hosts;
        readonly ITeamExplorerServices teServices;

        public EnsureLoggedInSection(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder,
            IConnectionManager cm, ITeamExplorerServices teServices)
            : base(serviceProvider, apiFactory, holder, cm)
        {
            IsVisible = false;
            this.hosts = new Lazy<IRepositoryHosts>(() => serviceProvider.TryGetService<IRepositoryHosts>());
            this.teServices = teServices;
        }

        public override void Initialize(IServiceProvider serviceProvider)
        {
            base.Initialize(serviceProvider);
            CheckLogin().Forget();
        }

        protected override void RepoChanged(bool changed)
        {
            base.RepoChanged(changed);
            CheckLogin().Forget();
        }

        async Task CheckLogin()
        {
            // this is not a github repo, or it hasn't been published yet
            if (ActiveRepo == null || ActiveRepoUri == null)
                return;

            var isgithub = await IsAGitHubRepo();
            if (!isgithub)
                return;

            teServices.ClearNotifications();
            var add = HostAddress.Create(ActiveRepoUri);
            bool loggedIn = await connectionManager.IsLoggedIn(hosts.Value, add);
            if (!loggedIn)
            {
                var msg = string.Format(CultureInfo.CurrentUICulture, Resources.NotLoggedInMessage, add.Title, add.Title);
                teServices.ShowMessage(
                    msg,
                    new Primitives.RelayCommand(_ => StartFlow(UIControllerFlow.Authentication))
                );
            }
        }

        void StartFlow(UIControllerFlow controllerFlow)
        {
            var uiProvider = ServiceProvider.GetService<IUIProvider>();
            var controller = uiProvider.Configure(controllerFlow);
            controller.TransitionSignal.Subscribe(c => { }, () => CheckLogin().Forget());
            uiProvider.RunInDialog(controller);
        }
    }
}