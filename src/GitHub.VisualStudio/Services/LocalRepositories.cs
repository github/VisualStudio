using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Helpers;
using GitHub.Models;

namespace GitHub.Services
{
    /// <summary>
    /// Stores a collection of known local repositories.
    /// </summary>
    /// <remarks>
    /// The list of repositories exposed here is the list of local repositories known to Team
    /// Explorer.
    /// </remarks>
    [Export(typeof(ILocalRepositories))]
    public class LocalRepositories : ILocalRepositories
    {
        readonly IVSGitServices vsGitServices;

        [ImportingConstructor]
        public LocalRepositories(IVSGitServices vsGitServices)
        {
            this.vsGitServices = vsGitServices;
        }

        /// <inheritdoc/>
        public async Task Refresh()
        {
            await ThreadingHelper.SwitchToPoolThreadAsync();
            var list = vsGitServices.GetKnownRepositories();
            await ThreadingHelper.SwitchToMainThreadAsync();

            repositories.Except(list).ToList().ForEach(x => repositories.Remove(x));
            list.Except(repositories).ToList().ForEach(x => repositories.Add(x));
        }

        readonly ObservableCollectionEx<LocalRepositoryModel> repositories
            = new ObservableCollectionEx<LocalRepositoryModel>();

        /// <inheritdoc/>
        public IReadOnlyObservableCollection<LocalRepositoryModel> Repositories => repositories;
    }
}
