using System.Windows.Controls;
using GitHub.Primitives;

namespace GitHub.Services
{
    public interface IVsDiffBase
    {
        void SetDiffBase(string repoPath, UriString cloneRepo, string branchName);
        object GetChangesList();
    }
}