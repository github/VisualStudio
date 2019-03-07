using System;
using System.Globalization;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.UI;

namespace GitHub.VisualStudio.TeamExplorer.Sync
{
    public class EnsureLoggedInSection : TeamExplorerSectionBase
    {
        readonly ITeamExplorerServices teServices;
        readonly IDialogService dialogService;

        public EnsureLoggedInSection(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder,
            IConnectionManager cm, ITeamExplorerServices teServices,
            IDialogService dialogService)
            : base(serviceProvider, apiFactory, holder, cm)
        {
            IsVisible = false;
            this.teServices = teServices;
            this.dialogService = dialogService;
        }

        public override void Initialize(IServiceProvider serviceProvider)
        {
            base.Initialize(serviceProvider);
            CheckLogin().Forget();
        }

        protected override void RepoChanged()
        {
            base.RepoChanged();
            CheckLogin().Forget();
        }

        async Task CheckLogin()
        {
            // this is not a github repo, or it hasn't been published yet
            if (ActiveRepo == null || ActiveRepoUri == null)
                return;

            var isgithub = await IsAGitHubRepo(ActiveRepoUri);
            if (!isgithub)
                return;

            teServices.ClearNotifications();
            var add = HostAddress.Create(ActiveRepoUri);
            bool loggedIn = await ConnectionManager.IsLoggedIn(add);
            if (!loggedIn)
            {
                var msg = string.Format(CultureInfo.CurrentCulture, Resources.NotLoggedInMessage, add.Title, add.Title);
                teServices.ShowMessage(
                    msg,
                    new Primitives.RelayCommand(_ => dialogService.ShowLoginDialog())
                );
            }
        }
    }
}