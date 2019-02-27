using System;
using System.Threading.Tasks;

namespace GitHub.Commands
{
    /// <summary>
    /// Sync submodules in local repository.
    /// </summary>
    public interface ISyncSubmodulesCommand : IVsCommand
    {
        /// <summary>
        /// Sync submodules in local repository.
        /// </summary>
        /// <returns>Tuple with bool that is true if command completed successfully and string with
        /// output from sync submodules Git command.</returns>
        Task<Tuple<bool, string>> SyncSubmodules();
    }
}