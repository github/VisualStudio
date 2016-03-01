using System;
using System.Globalization;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.VisualStudio
{
    public abstract class MenuBase
    {
        readonly IServiceProvider serviceProvider;
        protected IServiceProvider ServiceProvider { get { return serviceProvider; } }

        protected ISimpleRepositoryModel ActiveRepo { get; private set; }

        protected MenuBase()
        {
        }

        protected MenuBase(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
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

        protected bool IsGitHubRepo()
        {
            RefreshRepo();
            return ActiveRepo?.CloneUrl?.RepositoryName != null;
        }
    }
}
