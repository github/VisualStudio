using System;
using EnvDTE;
using EnvDTE80;
using GitHub.Services;
using LibGit2Sharp;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell.Interop;
using GitHub.Info;
using GitHub.Primitives;
using GitHub.Extensions;

namespace GitHub.VisualStudio
{

    public static class Services
    {
        public static IServiceProvider PackageServiceProvider { get; set; }

        /// <summary>
        /// Two ways of getting a service. First, trying the passed-in <paramref name="provider"/>,
        /// then <see cref="PackageServiceProvider"/>
        /// If the passed-in provider returns null, try PackageServiceProvider, returning the fetched value
        /// regardless of whether it's null or not.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Ret"></typeparam>
        /// <param name="provider"></param>
        /// <returns></returns>
        static Ret GetGlobalService<T, Ret>(IServiceProvider provider = null) where T : class where Ret : class
        {
            Ret ret = null;
            if (provider != null)
                ret = provider.GetService(typeof(T)) as Ret;
            if (ret != null)
                return ret;
            return PackageServiceProvider.GetService(typeof(T)) as Ret;
        }

        public static IComponentModel ComponentModel
        {
            get { return GetGlobalService<SComponentModel, IComponentModel>(); }
        }

        public static IVsWebBrowsingService GetWebBrowsingService(this IServiceProvider provider)
        {
            return GetGlobalService<SVsWebBrowsingService, IVsWebBrowsingService>(provider);
        }

        public static IVsOutputWindow OutputWindow
        {
            get { return GetGlobalService<SVsOutputWindow, IVsOutputWindow>(); }
        }

        static IVsOutputWindowPane outputWindowPane = null;
        public static IVsOutputWindowPane OutputWindowPane
        {
            get
            {
                if (outputWindowPane == null)
                {
                    // First make sure the output window is visible
                    var uiShell = GetGlobalService<SVsUIShell, IVsUIShell>();
                    // Get the frame of the output window
                    Guid outputWindowGuid = new Guid("{34e76e81-ee4a-11d0-ae2e-00a0c90fffc3}");
                    IVsWindowFrame outputWindowFrame = null;
                    ErrorHandler.ThrowOnFailure(uiShell.FindToolWindow((uint)__VSCREATETOOLWIN.CTW_fForceCreate, ref outputWindowGuid, out outputWindowFrame));
                    // Show the output window
                    if (outputWindowFrame != null)
                        ErrorHandler.ThrowOnFailure(outputWindowFrame.Show());

                    Guid paneGuid = new Guid("E37A42B1-C1AE-475C-9982-7F49FE61918D");
                    ErrorHandler.ThrowOnFailure(OutputWindow.CreatePane(ref paneGuid, ApplicationInfo.ApplicationSafeName, 1 /*visible=true*/, 0 /*clearWithSolution=false*/));
                    ErrorHandler.ThrowOnFailure(OutputWindow.GetPane(ref paneGuid, out outputWindowPane));
                }

                return outputWindowPane;
            }
        }

        public static DTE Dte
        {
            get { return GetGlobalService<DTE, DTE>(); }
        }

        public static DTE2 Dte2
        {
            get { return Dte as DTE2; }
        }

        public static IVsActivityLog GetActivityLog(this IServiceProvider provider)
        {
            return GetGlobalService<SVsActivityLog, IVsActivityLog>(provider);
        }

        public static IVsSolution GetSolution(this IServiceProvider provider)
        {
            return GetGlobalService<SVsSolution, IVsSolution>(provider);
        }

        public static UriString GetRepoUrlFromSolution(IVsSolution solution)
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
                return GetUri(repo);
            }
        }

        public static IRepository GetRepoFromSolution(this IVsSolution solution)
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
        static UriString GetUri(IRepository repo)
        {
            return UriString.ToUriString(GitService.GetUriFromRepository(repo)?.ToRepositoryUrl());
        }
        public static IGitService IGitService { get { return PackageServiceProvider.GetService<IGitService>(); } }
    }
}
