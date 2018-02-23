using GitHub.Api;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.UI;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Logging;
using Serilog;

namespace GitHub.VisualStudio
{
    public abstract class MenuBase
    {
        static readonly ILogger log = LogManager.ForContext<MenuBase>();
        readonly IGitHubServiceProvider serviceProvider;
        readonly Lazy<ISimpleApiClientFactory> apiFactory;
        readonly Lazy<IDialogService> dialogService;

        protected IGitHubServiceProvider ServiceProvider { get { return serviceProvider; } }

        protected ILocalRepositoryModel ActiveRepo { get; private set; }

        protected ISimpleApiClient simpleApiClient;

        protected ISimpleApiClient SimpleApiClient
        {
            get { return simpleApiClient; }
            set
            {
                if (simpleApiClient != value && value == null)
                    ApiFactory.ClearFromCache(simpleApiClient);
                simpleApiClient = value;
            }
        }

        protected ISimpleApiClientFactory ApiFactory => apiFactory.Value;
        protected IDialogService DialogService => dialogService.Value;

        protected MenuBase()
        {}

        protected MenuBase(IGitHubServiceProvider serviceProvider)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));

            this.serviceProvider = serviceProvider;
            apiFactory = new Lazy<ISimpleApiClientFactory>(() => ServiceProvider.TryGetService<ISimpleApiClientFactory>());
            dialogService = new Lazy<IDialogService>(() => ServiceProvider.TryGetService<IDialogService>());
        }

        protected ILocalRepositoryModel GetRepositoryByPath(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    var repo = ServiceProvider.TryGetService<IGitService>().GetRepository(path);
                    return new LocalRepositoryModel(repo.Info.WorkingDirectory.TrimEnd('\\'));
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error loading the repository from '{Path}'", path);
            }

            return null;
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
                    log.Error(ex, "Error loading the repository from '{Path}'", path);
                }
            }
            return activeRepo;
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
                    log.Error(ex, "Error loading the repository from '{Path}'", path);
                }
            }
        }

        protected async Task<bool> IsGitHubRepo()
        {
            RefreshRepo();

            var uri = ActiveRepo?.CloneUrl;
            if (uri == null)
                return false;

            SimpleApiClient = await ApiFactory.Create(uri);

            var isdotcom = HostAddress.IsGitHubDotComUri(uri.ToRepositoryUrl());
            if (!isdotcom)
            {
                var repo = await SimpleApiClient.GetRepository();
                var activeRepoFullName = ActiveRepo.Owner + '/' + ActiveRepo.Name;
                return (repo.FullName == activeRepoFullName || repo.Id == 0) && await SimpleApiClient.IsEnterprise();
            }
            return isdotcom;
        }
    }
}