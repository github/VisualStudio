using System.Windows.Controls;

namespace GitHub.Services
{
    public interface IVsDiffBase
    {
        void SetDiffBase(string repoPath, string branchName);
        object GetChangesList();
    }
}