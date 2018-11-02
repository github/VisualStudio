using System;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;

namespace GitHub.Services
{
    /// <summary>
    /// Stores a collection of known local repositories.
    /// </summary>
    public interface ILocalRepositories
    {
        /// <summary>
        /// Gets the currently known local repositories.
        /// </summary>
        IReadOnlyObservableCollection<LocalRepositoryModel> Repositories { get; }

        /// <summary>
        /// Updates <see cref="Repositories"/>.
        /// </summary>
        Task Refresh();
    }
}
