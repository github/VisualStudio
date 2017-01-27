using GitHub.Api;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.UI;
using NullGuard;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace GitHub.VisualStudio
{
    public abstract class MenuBase
    {
        readonly IGitHubServiceProvider serviceProvider;
        readonly Lazy<ISimpleApiClientFactory> apiFactory;

        protected IGitHubServiceProvider ServiceProvider { get { return serviceProvider; } }

        protected ILocalRepositoryModel ActiveRepo { get; private set; }

        protected ISimpleApiClient simpleApiClient;

        [AllowNull]
        protected ISimpleApiClient SimpleApiClient
        {
            [return: AllowNull]
            get { return simpleApiClient; }
            set
            {
                if (simpleApiClient != value && value == null)
                    ApiFactory.ClearFromCache(simpleApiClient);
                simpleApiClient = value;
            }
        }

        protected ISimpleApiClientFactory ApiFactory => apiFactory.Value;

        protected MenuBase()
        {}

        protected MenuBase(IGitHubServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            apiFactory = new Lazy<ISimpleApiClientFactory>(() => ServiceProvider.TryGetService<ISimpleApiClientFactory>());
        }

        protected ILocalRepositoryModel GetActiveRepo()
        {
            var activeRepo = ServiceProvider.TryGetService<ITeamExplorerServiceHolder>()?.ActiveRepo;
            // activeRepo can be null at this point because it is set elsewhere as the result of async operation that may not have completed yet.
            if (activeRepo == null)
            {
                var path = ServiceProvider.TryGetService<IVSGitServices>()?.GetActiveRepoPath() ?? String.Empty;
                try
                {
                    activeRepo = !string.IsNullOrEmpty(path) ? new LocalRepositoryModel(path) : null;
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
            IConnection connection = null;
            if (controllerFlow != UIControllerFlow.Authentication)
            {
                var activeRepo = GetActiveRepo();
                connection = ServiceProvider.TryGetService<IConnectionManager>()?.Connections
                    .FirstOrDefault(c => activeRepo?.CloneUrl?.RepositoryName != null && c.HostAddress.Equals(HostAddress.Create(activeRepo.CloneUrl)));
            }
            ServiceProvider.TryGetService<IUIProvider>().RunInDialog(controllerFlow, connection);
        }

        void RefreshRepo()
        {
            ActiveRepo = ServiceProvider.TryGetService<ITeamExplorerServiceHolder>().ActiveRepo;

            if (ActiveRepo == null)
            {
                var vsGitServices = ServiceProvider.TryGetService<IVSGitServices>();
                string path = vsGitServices?.GetActiveRepoPath() ?? String.Empty;
                try
                {
                    ActiveRepo = !String.IsNullOrEmpty(path) ? new LocalRepositoryModel(path) : null;
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

            SimpleApiClient = ApiFactory.Create(uri);

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