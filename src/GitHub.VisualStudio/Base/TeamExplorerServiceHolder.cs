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

namespace GitHub.VisualStudio.Base
{
    [Export(typeof(ITeamExplorerServiceHolder))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamExplorerServiceHolder : ITeamExplorerServiceHolder
    {
        Dictionary<object, Action<IServiceProvider>> handlers = new Dictionary<object, Action<IServiceProvider>>();

        IServiceProvider serviceProvider;
        bool notified = false;

        [AllowNull]
        public IServiceProvider ServiceProvider
        {
            [return: AllowNull] get { return serviceProvider; }
        }

        public void SetServiceProvider([AllowNull] IServiceProvider provider)
        {
            if (provider == null || serviceProvider == provider)
                return;

            serviceProvider = provider;
            notified = false;
        }

        public void ClearServiceProvider([AllowNull] IServiceProvider provider)
        {
            if (serviceProvider != provider)
                return;

            serviceProvider = null;
            handlers.Clear();

        }

        public void Notify()
        {
            notified = true;
            foreach (var handler in handlers.Values)
                handler(serviceProvider);
        }

        public void Subscribe(object who, Action<IServiceProvider> handler)
        {
            var provider = ServiceProvider;
            if (!handlers.ContainsKey(who))
                handlers.Add(who, handler);
            else
                handlers[who] = handler;

            if (provider != null && notified)
                handler(provider);
        }

        public void Unsubscribe(object who)
        {
            if (handlers.ContainsKey(who))
                handlers.Remove(who);
        }

        public TeamExplorerHome.GitHubHomeSection HomeSection
        {
            [return:AllowNull]
            get
            {
                if (ServiceProvider == null)
                    return null;
                var page = PageService;
                if (page == null)
                    return null;
                return page.GetSection(new Guid(TeamExplorerHome.GitHubHomeSection.GitHubHomeSectionId)) as TeamExplorerHome.GitHubHomeSection;
            }
        }

        ITeamExplorerPage PageService
        {
            [return:AllowNull]
            get { return ServiceProvider.GetService<ITeamExplorerPage>(); }
        }
    }
}
