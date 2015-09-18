using GitHub.Primitives;
using LibGit2Sharp;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using System;
using System.Linq;

namespace GitHub.Extensions
{
    public static class GitHelpers
    {
        public static Repository GetRepoFromIGit(this IGitRepositoryInfo repoInfo)
        {
            var repoPath = Repository.Discover(repoInfo.RepositoryPath);
            if (repoPath == null)
                return null;
            return new Repository(repoPath);
        }

        public static UriString GetUriFromRepository(this IGitRepositoryInfo repoInfo)
        {
            return repoInfo.GetRepoFromIGit()?.GetUri();
        }

        public static UriString GetUri(this Repository repo)
        {
            return UriString.ToUriString(GetUriFromRepository(repo)?.ToRepositoryUrl());
        }

        static UriString GetUriFromRepository(Repository repo)
        {
            return repo
                ?.Network
                .Remotes
                .FirstOrDefault(x => x.Name.Equals("origin", StringComparison.Ordinal))
                ?.Url;
        }

        public static Repository GetRepoFromPath(string path)
        {
            var repoPath = Repository.Discover(path);
            if (repoPath == null)
                return null;
            return new Repository(repoPath);
        }
    }
}
