using System;
using System.ComponentModel.Composition;
using GitHub.Extensions;
using GitHub.Services;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

namespace GitHub.VisualStudio.Base
{
    [Export(typeof(ITeamExplorerServiceHolder))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamExplorerServiceHolder : ITeamExplorerServiceHolder
    {
        IServiceProvider serviceProvider;

        /// <summary>
        /// This class relies on IVSGitExt that provides information when VS switches repositories.
        /// </summary>
        /// <param name="gitExt">Used for monitoring the active repository.</param>
        [ImportingConstructor]
        TeamExplorerServiceHolder(ITeamExplorerContext teamExplorerContext) :
            this(teamExplorerContext, ThreadHelper.JoinableTaskContext)
        {
        }

        /// <summary>
        /// This constructor can be used for unit testing.
        /// </summary>
        /// <param name="gitService">Used for monitoring the active repository.</param>
        /// <param name="joinableTaskContext">Used for switching to the Main thread.</param>
        public TeamExplorerServiceHolder(ITeamExplorerContext teamExplorerContext, JoinableTaskContext joinableTaskContext)
        {
            JoinableTaskCollection = joinableTaskContext.CreateCollection();
            JoinableTaskCollection.DisplayName = nameof(TeamExplorerServiceHolder);
            JoinableTaskFactory = joinableTaskContext.CreateFactory(JoinableTaskCollection);

            TeamExplorerContext = teamExplorerContext;
        }

        // set by the sections when they get initialized
        public IServiceProvider ServiceProvider
        {
            get { return serviceProvider; }
            set
            {
                if (serviceProvider == value)
                    return;

                serviceProvider = value;
                if (serviceProvider == null)
                    return;
            }
        }

        /// <summary>
        /// Clears the current ServiceProvider if it matches the one that is passed in.
        /// This is usually called on Dispose, which might happen after another section
        /// has changed the ServiceProvider to something else, which is why we require
        /// the parameter to match.
        /// </summary>
        /// <param name="provider">If the current ServiceProvider matches this, clear it</param>
        public void ClearServiceProvider(IServiceProvider provider)
        {
            Guard.ArgumentNotNull(provider, nameof(provider));

            if (serviceProvider != provider)
                return;

            ServiceProvider = null;
        }

        public IGitAwareItem HomeSection
        {
            get
            {
                if (ServiceProvider == null)
                    return null;
                var page = PageService;
                if (page == null)
                    return null;
                return page.GetSection(new Guid(TeamExplorer.Home.GitHubHomeSection.GitHubHomeSectionId)) as IGitAwareItem;
            }
        }

        ITeamExplorerPage PageService
        {
            get { return ServiceProvider.GetServiceSafe<ITeamExplorerPage>(); }
        }

        public JoinableTaskCollection JoinableTaskCollection { get; }
        public JoinableTaskFactory JoinableTaskFactory { get; }
        public ITeamExplorerContext TeamExplorerContext { get; }
    }
}
