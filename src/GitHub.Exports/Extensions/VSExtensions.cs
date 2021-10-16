using GitHub.Services;
using LibGit2Sharp;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitHub.Extensions
{
    public static class ISolutionExtensions
    {
        public static IRepository GetRepositoryFromSolution(this IVsSolution solution)
        {
            string solutionDir, solutionFile, userFile;
            if (!ErrorHandler.Succeeded(solution.GetSolutionInfo(out solutionDir, out solutionFile, out userFile)))
                return null;
            if (solutionDir == null)
                return null;
            return GitService.GitServiceHelper.GetRepository(solutionDir);
        }
    }
}
