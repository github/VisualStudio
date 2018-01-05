using System;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.Composition;
using GitHub.Models;

namespace GitHub.Services
{
    [Export(typeof(ITeamExplorerContext))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamExplorerContext : ITeamExplorerContext
    {
        readonly IVSGitExt vsGitExt;
        readonly ITeamExplorerServiceHolder teamExplorerServiceHolder;

        [ImportingConstructor]
        public TeamExplorerContext(IVSGitExt vsGitExt, ITeamExplorerServiceHolder teamExplorerServiceHolder)
        {
            this.vsGitExt = vsGitExt;
            this.teamExplorerServiceHolder = teamExplorerServiceHolder;

            vsGitExt.ActiveRepositoriesChanged += () =>
            {
                StatusChanged?.Invoke(this, EventArgs.Empty);
            };
        }

        public ILocalRepositoryModel GetActiveRepository()
        {
            var activeRepository = vsGitExt.ActiveRepositories.FirstOrDefault();
            if (activeRepository == null)
            {
                // HACK: TeamExplorerServiceHolder does some magic to ensure that ActiveRepositories is aviablable.
                activeRepository = teamExplorerServiceHolder.ActiveRepo;
            }

            return activeRepository;
        }

        public event EventHandler StatusChanged;
    }
}
