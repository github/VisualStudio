using GitHub.Primitives;
using LibGit2Sharp;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.Services
{
    public interface IGitService
    {
        UriString GetUri(string path);
        UriString GetUri(IRepository repo);
        UriString GetUri(IGitRepositoryInfo repoInfo);
        IRepository GetRepo(IGitRepositoryInfo repoInfo);
        IRepository GetRepo(string path);
    }

    [Export(typeof(IVSServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GitService : IGitService
    {
        public UriString GetUri(IRepository repo)
        {
            return UriString.ToUriString(GetUriFromRepository(repo)?.ToRepositoryUrl());
        }

        public UriString GetUri(string path)
        {
            return GetUri(GetRepo(path));
        }

        public UriString GetUri(IGitRepositoryInfo repoInfo)
        {
            return GetUri(GetRepo(repoInfo));
        }

        public IRepository GetRepo(IGitRepositoryInfo repoInfo)
        {
            var repoPath = Repository.Discover(repoInfo?.RepositoryPath);
            if (repoPath == null)
                return null;
            return new Repository(repoPath);
        }

        public IRepository GetRepo(string path)
        {
            var repoPath = Repository.Discover(path);
            if (repoPath == null)
                return null;
            return new Repository(repoPath);
        }

        internal static UriString GetUriFromRepository(IRepository repo)
        {
            return repo
                ?.Network
                .Remotes
                .FirstOrDefault(x => x.Name.Equals("origin", StringComparison.Ordinal))
                ?.Url;
        }
    }
}
