using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Services;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using Microsoft.TeamFoundation.Controls;
using GitHub.UI;
using GitHub.VisualStudio.UI;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Extensions;
using GitHub.Logging;
using Serilog;
using System.Collections.Specialized;
using GitHub.Settings;

namespace GitHub.VisualStudio.TeamExplorer.Home
{
    [TeamExplorerNavigationItem(ForkNavigationItemId, NavigationItemPriority.Fork)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ForkNavigationItem : TeamExplorerNavigationItemBase
    {
        static readonly ILogger log = LogManager.ForContext<ForkNavigationItem>();

        public const string ForkNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA6";

        readonly IDialogService dialogService;
        IConnectionManager connectionManager;

        [ImportingConstructor]
        public ForkNavigationItem(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory,
            ITeamExplorerServiceHolder holder,
            IDialogService dialogService,
            IPackageSettings packageSettings)
            : base(serviceProvider, apiFactory, holder, Octicon.repo_forked)
        {
            this.dialogService = dialogService;

            Text = Resources.ForkNavigationItemText;
            ArgbColor = Colors.PurpleNavigationItem.ToInt32();
            ConnectionManager.Connections.CollectionChanged += ConnectionsChanged;

            IsVisible = packageSettings?.ForkButton ?? false;
            packageSettings.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(packageSettings.ForkButton))
                {
                    IsVisible = packageSettings.ForkButton;
                }
            };
        }

        IConnectionManager ConnectionManager
        {
            get
            {
                // We can't receive IConnectionManager in the constructor because Invalidate()
                // is called from the base constructor and so we don't have chance to save it
                // to a field.
                if (connectionManager == null)
                {
                    connectionManager = ServiceProvider.GetService<IConnectionManager>();
                }

                return connectionManager;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            ConnectionManager.Connections.CollectionChanged -= ConnectionsChanged;
        }

        public override async void Execute()
        {
            var connection = await connectionManager.GetConnection(ActiveRepo);

            if (connection?.IsLoggedIn == true)
            {
                await dialogService.ShowForkDialog(ActiveRepo, connection);
            }
        }

        public override async void Invalidate()
        {
            try
            {
                IsVisible = false;

                if (await IsAGitHubRepo())
                {
                    var connection = await ConnectionManager.GetConnection(ActiveRepo);
                    IsVisible = connection?.IsLoggedIn ?? false;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error updating ForkNavigationItem visibility");
            }
        }

        private void ConnectionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Invalidate();
        }
    }
}
