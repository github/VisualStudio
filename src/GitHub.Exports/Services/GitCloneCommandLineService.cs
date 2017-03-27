using System;
using System.IO;
using System.Diagnostics;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using GitHub.Primitives;
using Rothko;

namespace GitHub.Services
{
    [Export(typeof(IGitCloneCommandLineService))]
    public class GitCloneCommandLineService : IGitCloneCommandLineService
    {
        IVsAppCommandLine vsAppCommandLine;
        IVSGitServices vsGitServices;
        IVSServices vsServices;
        IOperatingSystem operatingSystem;

        [ImportingConstructor]
        internal GitCloneCommandLineService(IGitHubServiceProvider sp, IVSGitServices vsGitServices,
            ITeamExplorerServices teamExplorerServices, IVSServices vsServices, IOperatingSystem operatingSystem)
            : this(sp.TryGetService<IVsAppCommandLine>(), vsGitServices, teamExplorerServices, vsServices, operatingSystem)
        {
        }

        public GitCloneCommandLineService(IVsAppCommandLine vsAppCommandLine, IVSGitServices vsGitServices,
            ITeamExplorerServices teamExplorerServices, IVSServices vsServices, IOperatingSystem operatingSystem)
        {
            this.vsAppCommandLine = vsAppCommandLine;
            this.vsGitServices = vsGitServices;
            this.vsServices = vsServices;
            this.operatingSystem = operatingSystem;
        }

        public string FindGitCloneOption()
        {

            if (vsAppCommandLine == null)
            {
                return null;
            }

            int isPresent;
            string optionValue;
            if (ErrorHandler.Failed(vsAppCommandLine.GetOption("GitClone", out isPresent, out optionValue)))
            {
                return null;
            }

            if (isPresent == 0)
            {
                return null;
            }

            return optionValue;
        }

        public bool TryOpenRepository(string cloneUrl)
        {
            if(TryOpenLocalClonePath(cloneUrl))
            {
                return true;
            }

            if(TryOpenKnownRepository(cloneUrl))
            {
                return true;
            }

            return false;
        }

        bool TryOpenKnownRepository(string cloneUrl)
        {
            var repos = vsGitServices.GetKnownRepositories();
            foreach (var repo in repos)
            {
                if (cloneUrl == repo.CloneUrl)
                {
                    if(vsServices.TryOpenRepository(repo.LocalPath))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool TryOpenLocalClonePath(string cloneUrl)
        {
            var cloneUri = FindGitHubCloneUri(cloneUrl);
            if(cloneUri == null)
            {
                return false;
            }

            var clonePath = vsGitServices.GetLocalClonePathFromGitProvider();
            if(clonePath == null)
            {
                return false;
            }

            var repoPath = Path.Combine(clonePath, cloneUri.Owner, cloneUri.RepositoryName);
            var repoDir = operatingSystem.Directory.GetDirectory(repoPath);
            if (!repoDir.Exists)
            {
                return false;
            }

            return vsServices.TryOpenRepository(repoPath);
        }

        static UriString FindGitHubCloneUri(string cloneUrl)
        {
            try
            {
                var uriString = new UriString(cloneUrl);
                if (uriString.Host != "github.com")
                {
                    return null;
                }

                if(string.IsNullOrEmpty(uriString.Owner) || string.IsNullOrEmpty(uriString.RepositoryName))
                {
                    return null;
                }

                return uriString;
            }
            catch(Exception e)
            {
                Trace.WriteLine(e);
                return null;
            }
        }
    }
}