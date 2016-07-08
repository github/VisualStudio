using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using System.Threading.Tasks;
using GitHub.Api;
using NullGuard;
using GitHub.UI;

namespace GitHub.VisualStudio
{
    public abstract class MenuBase
    {
        readonly IServiceProvider serviceProvider;
        readonly ISimpleApiClientFactory apiFactory;

        protected IServiceProvider ServiceProvider { get { return serviceProvider; } }

        protected ISimpleRepositoryModel ActiveRepo { get; private set; }

        protected ISimpleApiClient simpleApiClient;

        [AllowNull]
        protected ISimpleApiClient SimpleApiClient
        {
            [return: AllowNull]
            get { return simpleApiClient; }
            set
            {
                if (simpleApiClient != value && value == null)
                    apiFactory.ClearFromCache(simpleApiClient);
                simpleApiClient = value;
            }
        }

        protected ISimpleApiClientFactory ApiFactory => apiFactory;

        protected MenuBase()
        {
        }

        protected MenuBase(IServiceProvider serviceProvider, ISimpleApiClientFactory apiFactory)
        {
            this.serviceProvider = serviceProvider;
            this.apiFactory = apiFactory;
        }

        protected MenuBase(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected ISimpleRepositoryModel GetActiveRepo()
        {
            var activeRepo = ServiceProvider.GetExportedValue<ITeamExplorerServiceHolder>()?.ActiveRepo;
            // activeRepo can be null at this point because it is set elsewhere as the result of async operation that may not have completed yet.
            if (activeRepo == null)
            {
                var path = ServiceProvider.GetExportedValue<IVSServices>()?.GetActiveRepoPath() ?? String.Empty;
                try
                {
                    activeRepo = !string.IsNullOrEmpty(path) ? new SimpleRepositoryModel(path) : null;
                }
                catch (Exception ex)
                {
                    VsOutputLogger.WriteLine(string.Format(CultureInfo.CurrentCulture, "Error loading the repository from '{0}'. {1}", path, ex));
                }
            }
            return activeRepo;
        }

        protected void StartFlow(UIControllerFlow controllerFlow)
        {
            var uiProvider = ServiceProvider.GetExportedValue<IUIProvider>();
            Debug.Assert(uiProvider != null, "MenuBase:StartFlow:No UIProvider available.");
            if (uiProvider == null)
                return;

            IConnection connection = null;
            if (controllerFlow != UIControllerFlow.Authentication)
            {
                var activeRepo = GetActiveRepo();
                connection = ServiceProvider.GetExportedValue<IConnectionManager>()?.Connections
                    .FirstOrDefault(c => activeRepo?.CloneUrl?.RepositoryName != null && c.HostAddress.Equals(HostAddress.Create(activeRepo.CloneUrl)));
            }
            uiProvider.RunUI(controllerFlow, connection);
        }

        void RefreshRepo()
        {
            ActiveRepo = ServiceProvider.GetExportedValue<ITeamExplorerServiceHolder>().ActiveRepo;

            if (ActiveRepo == null)
            {
                var vsservices = ServiceProvider.GetExportedValue<IVSServices>();
                string path = vsservices?.GetActiveRepoPath() ?? String.Empty;
                try
                {
                    ActiveRepo = !String.IsNullOrEmpty(path) ? new SimpleRepositoryModel(path) : null;
                }
                catch (Exception ex)
                {
                    VsOutputLogger.WriteLine(string.Format(CultureInfo.CurrentCulture, "{0}: Error loading the repository from '{1}'. {2}", GetType(), path, ex));
                }
            }
        }

        protected async Task<bool> IsGitHubRepo()
        {
            RefreshRepo();

            var uri = ActiveRepo?.CloneUrl;
            if (uri == null)
                return false;

            Debug.Assert(apiFactory != null, "apiFactory cannot be null. Did you call the right constructor?");
            SimpleApiClient = apiFactory.Create(uri);

            var isdotcom = HostAddress.IsGitHubDotComUri(uri.ToRepositoryUrl());
            if (!isdotcom)
            {
                var repo = await SimpleApiClient.GetRepository();
                return (repo.FullName == ActiveRepo.Name || repo.Id == 0) && SimpleApiClient.IsEnterprise();
            }
            return isdotcom;
        }
    }
}