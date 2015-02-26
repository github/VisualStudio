using System;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using GitHub.Services;
using LibGit2Sharp;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using NullGuard;

namespace GitHub.VisualStudio
{
    public static class Services
    {
        public static IComponentModel ComponentModel
        {
            get { return Package.GetGlobalService(typeof(SComponentModel)) as IComponentModel; }
        }

        public static IVsWebBrowsingService WebBrowsingService
        {
            get { return Package.GetGlobalService(typeof(SVsWebBrowsingService)) as IVsWebBrowsingService; }
        }

        public static IVsTextManager TextManager
        {
            get { return Package.GetGlobalService(typeof(SVsTextManager)) as IVsTextManager; }
        }

        public static IVsOutputWindow OutputWindow
        {
            get { return Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow; }
        }

        public static IVsSolution Solution
        {
            get { return Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution; }
        }

        public static IVsWebBrowsingService WebBrowsing
        {
            get { return Package.GetGlobalService(typeof(SVsWebBrowsingService)) as IVsWebBrowsingService; }
        }

        public static IVsUIShell Shell
        {
            get { return Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell; }
        }

        public static DTE Dte
        {
            get { return Package.GetGlobalService(typeof(DTE)) as DTE; }
        }

        public static DTE2 Dte2
        {
            get { return Dte as DTE2; }
        }

        public static IVsSolution GetSolution(this IServiceProvider provider)
        {
            return provider.GetService(typeof(SVsSolution)) as IVsSolution;
        }

        public static IVsWebBrowsingService GetWebBrowsing(this IServiceProvider provider)
        {
            return provider.GetService(typeof(SVsWebBrowsingService)) as IVsWebBrowsingService;
        }

        [return: AllowNull]
        public static T GetExportedValue<T>(this IServiceProvider serviceProvider)
        {
            var ui = serviceProvider as IUIProvider;
            if (ui != null)
                return ui.GetService<T>();
            else
            {
                var componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
                return componentModel.DefaultExportProvider.GetExportedValue<T>();
            }
        }

        [return: AllowNull]
        public static Uri GetRepoUrlFromSolution(IVsSolution solution)
        {
            string solutionDir, solutionFile, userFile;
            if (!ErrorHandler.Succeeded(solution.GetSolutionInfo(out solutionDir, out solutionFile, out userFile)))
                return null;
            if (solutionDir == null)
                return null;
            var repoPath = Repository.Discover(solutionDir);
            if (repoPath == null)
                return null;
            using (var repo = new Repository(repoPath))
            {
                return GetUriFromRepository(repo);
            }
        }

        [return: AllowNull]
        public static Repository GetRepoFromSolution(IVsSolution solution)
        {
            string solutionDir, solutionFile, userFile;
            if (!ErrorHandler.Succeeded(solution.GetSolutionInfo(out solutionDir, out solutionFile, out userFile)))
                return null;
            if (solutionDir == null)
                return null;
            var repoPath = Repository.Discover(solutionDir);
            if (repoPath == null)
                return null;
            return new Repository(repoPath);
        }

        [return: AllowNull]
        public static Uri GetUriFromRepository(Repository repo)
        {
            var remote = repo.Network.Remotes.FirstOrDefault(x => x.Name.Equals("origin", StringComparison.Ordinal));
            if (remote == null)
                return null;
            Uri uri;
            var url = remote.Url;
            // fixup ssh urls
            if (url.StartsWith("git@github.com:", StringComparison.Ordinal))
                url = url.Replace("git@github.com:", "https://github.com/");
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                return null;
            return uri;
        }
    }
}
