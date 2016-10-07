using System.Collections.Generic;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IVSGitServices
    {
        string GetLocalClonePathFromGitProvider();
        void Clone(string cloneUrl, string clonePath, bool recurseSubmodules);
        string GetActiveRepoPath();
        LibGit2Sharp.IRepository GetActiveRepo();
        IEnumerable<ILocalRepositoryModel> GetKnownRepositories();
        string SetDefaultProjectPath(string path);
    }
}