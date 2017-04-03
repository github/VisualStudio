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
        public const string GitCloneSwitch = "GitClone";

        IVsAppCommandLine vsAppCommandLine;
        IVSGitServices vsGitServices;
        IOperatingSystem operatingSystem;

        [ImportingConstructor]
        internal GitCloneCommandLineService(IGitHubServiceProvider sp, IVSGitServices vsGitServices,
            ITeamExplorerServices teamExplorerServices, IVSServices vsServices, IOperatingSystem operatingSystem)
            : this(sp.TryGetService<IVsAppCommandLine>(), vsGitServices, teamExplorerServices, operatingSystem)
        {
        }

        public GitCloneCommandLineService(IVsAppCommandLine vsAppCommandLine, IVSGitServices vsGitServices,
            ITeamExplorerServices teamExplorerServices, IOperatingSystem operatingSystem)
        {
            this.vsAppCommandLine = vsAppCommandLine;
            this.vsGitServices = vsGitServices;
            this.operatingSystem = operatingSystem;
        }

        public UriString FindGitHubCloneOption()
        {
            if (vsAppCommandLine == null)
            {
                return null;
            }

            int isPresent;
            string optionValue;
            if (ErrorHandler.Failed(vsAppCommandLine.GetOption(GitCloneSwitch, out isPresent, out optionValue)))
            {
                return null;
            }

            if (isPresent == 0)
            {
                return null;
            }

            return FindGitHubCloneUri(optionValue);
        }

        public bool TryOpenRepository(UriString cloneUri)
        {
            if (TryOpenLocalClonePath(cloneUri))
            {
                return true;
            }

            if (TryOpenKnownRepository(cloneUri))
            {
                return true;
            }

            return false;
        }

        bool TryOpenKnownRepository(UriString cloneUri)
        {
            var repos = vsGitServices.GetKnownRepositories();
            foreach (var repo in repos)
            {
                if (cloneUri.ToString() == repo.CloneUrl)
                {
                    if (vsGitServices.TryOpenRepository(repo.LocalPath))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool TryOpenLocalClonePath(UriString cloneUri)
        {
            var clonePath = vsGitServices.GetLocalClonePathFromGitProvider();
            if (clonePath == null)
            {
                return false;
            }

            var repoPath = Path.Combine(clonePath, cloneUri.Owner, cloneUri.RepositoryName);
            var repoDir = operatingSystem.Directory.GetDirectory(repoPath);
            if (!repoDir.Exists)
            {
                return false;
            }

            return vsGitServices.TryOpenRepository(repoPath);
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

                if (string.IsNullOrEmpty(uriString.Owner) || string.IsNullOrEmpty(uriString.RepositoryName))
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