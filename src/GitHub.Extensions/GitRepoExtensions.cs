using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using NullGuard;
using System;

namespace GitHub.Extensions
{
    public static class GitRepoExtensions
    {
        public static bool Compare([AllowNull] this IGitRepositoryInfo lhs, [AllowNull]IGitRepositoryInfo rhs)
        {
            if (lhs == null && rhs == null)
                return true;
            if (lhs != null && rhs != null)
                return String.Equals(lhs.RepositoryPath, rhs.RepositoryPath, StringComparison.CurrentCultureIgnoreCase);
            return false;
        }
    }
}
