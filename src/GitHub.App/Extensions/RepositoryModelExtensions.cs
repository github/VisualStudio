using GitHub.Models;
using GitHub.VisualStudio;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using NullGuard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.Extensions
{
    public static class RepositoryModelExtensions
    {
        [return:AllowNull]
        public static ISimpleRepositoryModel ToModel([AllowNull] this IGitRepositoryInfo repo)
        {
            if (repo == null)
                return null;
            var uri = repo.GetUriFromRepository();
            var name = uri?.NameWithOwner;
            return name != null ? new SimpleRepositoryModel(name, uri, repo.RepositoryPath) : null;
        }
    }
}
