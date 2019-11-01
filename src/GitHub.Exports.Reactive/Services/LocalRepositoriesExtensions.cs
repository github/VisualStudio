using System;
using GitHub.Models;
using GitHub.Primitives;
using ReactiveUI;

namespace GitHub.Services
{
    /// <summary>
    /// Implements extension methods for <see cref="ILocalRepositories"/>.
    /// </summary>
    public static class LocalRepositoriesExtensions
    {
        /// <summary>
        /// Gets a derived collection that contains all known repositories with the specified
        /// clone URL, ordered by name.
        /// </summary>
        /// <param name="repos">The local repositories object.</param>
        /// <param name="address">The address.</param>
        public static IReactiveDerivedList<LocalRepositoryModel> GetRepositoriesForAddress(
            this ILocalRepositories repos,
            HostAddress address)
        {
            return repos.Repositories.CreateDerivedCollection(
                x => x,
                x => x.CloneUrl != null && address.Equals(HostAddress.Create(x.CloneUrl)),
                OrderedComparer<LocalRepositoryModel>.OrderBy(x => x.Name).Compare);
        }
    }
}
