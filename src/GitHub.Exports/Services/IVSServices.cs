using System.Collections.Generic;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IVSServices
    {
        string GetLocalClonePathFromGitProvider();
        void Clone(string cloneUrl, string clonePath, bool recurseSubmodules);
        string GetActiveRepoPath();
        LibGit2Sharp.IRepository GetActiveRepo();
        IEnumerable<ISimpleRepositoryModel> GetKnownRepositories();
        string SetDefaultProjectPath(string path);

        void ActivityLogMessage(string message);
        void ActivityLogWarning(string message);
        void ActivityLogError(string message);
    }
}