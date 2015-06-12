using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.VisualStudio.Helpers;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using NullGuard;
using GitHub.Extensions;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;

namespace GitHub.VisualStudio.Base
{
    [Export(typeof(ITeamExplorerServiceHolder))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamExplorerServiceHolder : ITeamExplorerServiceHolder
    {
        Dictionary<object, Action<IServiceProvider>> serviceProviderHandlers = new Dictionary<object, Action<IServiceProvider>>();
        Dictionary<object, Action<IGitRepositoryInfo>> activeRepoHandlers = new Dictionary<object, Action<IGitRepositoryInfo>>();

        IServiceProvider serviceProvider;
        IGitRepositoryInfo activeRepo;
        bool serviceProviderNotified = false;
        bool activeRepoNotified = false;

        [AllowNull]
        public IServiceProvider ServiceProvider
        {
            [return: AllowNull] get { return serviceProvider; }
        }

        [AllowNull]
        public IGitRepositoryInfo ActiveRepo
        {
            [return: AllowNull] get { return activeRepo; }
        }

        public void SetServiceProvider([AllowNull] IServiceProvider provider)
        {
            if (provider == null || serviceProvider == provider)
                return;

            serviceProvider = provider;
            serviceProviderNotified = false;
        }

        public void ClearServiceProvider([AllowNull] IServiceProvider provider)
        {
            if (serviceProvider != provider)
                return;

            serviceProvider = null;
            serviceProviderHandlers.Clear();
        }

        public void SetActiveRepo([AllowNull] IGitRepositoryInfo repo)
        {
            if (repo == null || activeRepo == repo)
                return;

            activeRepo = repo;
            activeRepoNotified = false;
        }

        public void ClearActiveRepo([AllowNull] IGitRepositoryInfo repo)
        {
            if (activeRepo != repo)
                return;

            activeRepo = null;
            activeRepoHandlers.Clear();
        }

        public void NotifyServiceProvider()
        {
            serviceProviderNotified = true;
            foreach (var handler in serviceProviderHandlers.Values)
                handler(serviceProvider);
        }

        public void NotifyActiveRepo()
        {
            activeRepoNotified = true;
            foreach (var handler in activeRepoHandlers.Values)
                handler(activeRepo);
        }

        public void Subscribe(object who, Action<IServiceProvider> handler)
        {
            var provider = ServiceProvider;
            if (!serviceProviderHandlers.ContainsKey(who))
                serviceProviderHandlers.Add(who, handler);
            else
                serviceProviderHandlers[who] = handler;

            if (provider != null && serviceProviderNotified)
                handler(provider);
        }

        public void Subscribe(object who, Action<IGitRepositoryInfo> handler)
        {
            var repo = ActiveRepo;
            if (!activeRepoHandlers.ContainsKey(who))
                activeRepoHandlers.Add(who, handler);
            else
                activeRepoHandlers[who] = handler;

            if (repo != null && activeRepoNotified)
                handler(repo);
        }

        public void Unsubscribe(object who)
        {
            if (serviceProviderHandlers.ContainsKey(who))
                serviceProviderHandlers.Remove(who);
            if (activeRepoHandlers.ContainsKey(who))
                activeRepoHandlers.Remove(who);
        }

        public IGitAwareItem HomeSection
        {
            [return:AllowNull]
            get
            {
                if (ServiceProvider == null)
                    return null;
                var page = PageService;
                if (page == null)
                    return null;
                return page.GetSection(new Guid(TeamExplorerHome.GitHubHomeSection.GitHubHomeSectionId)) as IGitAwareItem;
            }
        }

        ITeamExplorerPage PageService
        {
            [return:AllowNull]
            get { return ServiceProvider.GetService<ITeamExplorerPage>(); }
        }
    }
}
