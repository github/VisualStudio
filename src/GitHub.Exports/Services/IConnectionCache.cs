using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.Services
{
    /// <summary>
    /// Loads the configured connections from  a cache.
    /// </summary>
    public interface IConnectionCache
    {
        /// <summary>
        /// Loads the configured connections.
        /// </summary>
        /// <returns>A task returning the collection of configured collections.</returns>
        Task<IEnumerable<ConnectionDetails>> Load();

        /// <summary>
        /// Saves the configured connections.
        /// </summary>
        /// <param name="connections">The collection of configured collections to save.</param>
        /// <returns>A task tracking the operation.</returns>
        Task Save(IEnumerable<ConnectionDetails> connections);
    }
}
