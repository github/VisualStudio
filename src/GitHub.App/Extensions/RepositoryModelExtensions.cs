using GitHub.Models;
using GitHub.VisualStudio;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using NullGuard;

namespace GitHub.Extensions
{
    public static class RepositoryModelExtensions
    {
        /// <summary>
        /// Create a SimpleRepositoryModel from a VS git repo object
        /// </summary>
        [return:AllowNull]
        public static ISimpleRepositoryModel ToModel([AllowNull] this IGitRepositoryInfo repo)
        {
            if (repo == null)
                return null;
            return SimpleRepositoryModel.Create(repo.RepositoryPath);
        }
    }
}
