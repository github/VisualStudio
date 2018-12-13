using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IVSGitServices
    {
        string GetLocalClonePathFromGitProvider();

        /// <summary>
        /// Clones a repository via Team Explorer.
        /// </summary>
        /// <param name="cloneUrl">The URL of the repository to clone.</param>
        /// <param name="clonePath">The path to clone the repository to.</param>
        /// <param name="recurseSubmodules">Whether to recursively clone submodules.</param>
        /// <param name="progress">
        /// An object through which to report progress. This must be of type
        /// System.IProgress&lt;Microsoft.VisualStudio.Shell.ServiceProgressData&gt;, but
        /// as that type is only available in VS2017+ it is typed as <see cref="object"/> here.
        /// </param>
        /// <seealso cref="System.IProgress{T}"/>
        /// <seealso cref="Microsoft.VisualStudio.Shell.ServiceProgressData"/>
        Task Clone(
            string cloneUrl,
            string clonePath,
            bool recurseSubmodules,
            object progress = null);

        string GetActiveRepoPath();
        LibGit2Sharp.IRepository GetActiveRepo();
        IEnumerable<LocalRepositoryModel> GetKnownRepositories();
        string SetDefaultProjectPath(string path);
    }
}