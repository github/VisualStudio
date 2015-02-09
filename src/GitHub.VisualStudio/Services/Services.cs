using LibGit2Sharp;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.VisualStudio
{
    public static class Services
    {
        public static IVsSolution Solution
        {
            get { return Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution; }
        }

        public static IVsSolution GetSolution(this IServiceProvider provider)
        {
            return provider.GetService(typeof(SVsSolution)) as IVsSolution;
        }

        public static async Task<bool> IsHostedOnGitHub(this IVsSolution solution)
        {
            string solutionDir, solutionFile, userFile;
            if (!ErrorHandler.Succeeded(solution.GetSolutionInfo(out solutionDir, out solutionFile, out userFile)))
                return false;
            if (solutionDir == null)
                return false;
            var repoPath = Repository.Discover(solutionDir);
            if (repoPath == null)
                return false;
            using (var repo = new Repository(repoPath))
            {
                var remote = repo.Network.Remotes.FirstOrDefault(x => x.Name.Equals("origin", StringComparison.InvariantCulture));
                if (remote == null)
                    return false;
                Uri uri;
                var url = remote.Url;
                // fixup ssh urls
                if (url.StartsWith("git@github.com:"))
                    url = url.Replace("git@github.com:", "https://github.com/");
                if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                    return false;

                if (HostAddress.IsGitHubDotComUri(uri))
                    return true;
                // enterprise probe
            }
            return false;

        }
    }
}
