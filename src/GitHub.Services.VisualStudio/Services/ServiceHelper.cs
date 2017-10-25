using System;
using EnvDTE;
using EnvDTE80;
using GitHub.Info;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace GitHub.Services
{
    public static class ServiceHelper
    {
        /// <summary>
        /// Gets a service provider which can be used by unit tests to inject services.
        /// </summary>
        public static IServiceProvider UnitTestServiceProvider { get; set; }

        /// <summary>
        /// Three ways of getting a service. First, trying the passed-in <paramref name="provider"/>,
        /// then <see cref="UnitTestServiceProvider"/>, then <see cref="T:Microsoft.VisualStudio.Shell.Package"/>
        /// If the passed-in provider returns null, try PackageServiceProvider or Package, returning the fetched value
        /// regardless of whether it's null or not. Package.GetGlobalService is never called if PackageServiceProvider is set.
        /// This is on purpose, to support easy unit testing outside VS.
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
            if (UnitTestServiceProvider != null)
                return UnitTestServiceProvider.GetService(typeof(T)) as Ret;
            return Package.GetGlobalService(typeof(T)) as Ret;
        }

        public static IGitHubServiceProvider GitHubServiceProvider => GetGlobalService<IGitHubServiceProvider, IGitHubServiceProvider>();

        public static IComponentModel ComponentModel => GetGlobalService<SComponentModel, IComponentModel>();
        public static ExportProvider DefaultExportProvider => ComponentModel.DefaultExportProvider;

        public static IVsWebBrowsingService GetWebBrowsingService(this IServiceProvider provider)
        {
            return GetGlobalService<SVsWebBrowsingService, IVsWebBrowsingService>(provider);
        }

        public static IVsOutputWindow OutputWindow => GetGlobalService<SVsOutputWindow, IVsOutputWindow>();

        static IVsOutputWindowPane outputWindowPane;
        public static IVsOutputWindowPane OutputWindowPane
        {
            get
            {
                if (outputWindowPane == null)
                {
                    // First make sure the output window is visible
                    var uiShell = GetGlobalService<SVsUIShell, IVsUIShell>();
                    // Get the frame of the output window
                    var outputWindowGuid = new Guid("{34e76e81-ee4a-11d0-ae2e-00a0c90fffc3}");
                    IVsWindowFrame outputWindowFrame;
                    ErrorHandler.ThrowOnFailure(uiShell.FindToolWindow((uint)__VSCREATETOOLWIN.CTW_fForceCreate, ref outputWindowGuid, out outputWindowFrame));
                    // Show the output window
                    if (outputWindowFrame != null)
                        ErrorHandler.ThrowOnFailure(outputWindowFrame.Show());

                    var paneGuid = new Guid("E37A42B1-C1AE-475C-9982-7F49FE61918D");
                    ErrorHandler.ThrowOnFailure(OutputWindow.CreatePane(ref paneGuid, ApplicationInfo.ApplicationSafeName, 1 /*visible=true*/, 0 /*clearWithSolution=false*/));
                    ErrorHandler.ThrowOnFailure(OutputWindow.GetPane(ref paneGuid, out outputWindowPane));
                }

                return outputWindowPane;
            }
        }

        public static DTE Dte => GetGlobalService<DTE, DTE>();

        // ReSharper disable once SuspiciousTypeConversion.Global
        public static DTE2 Dte2 => Dte as DTE2;

        public static IVsUIShell UIShell => GetGlobalService<SVsUIShell, IVsUIShell>();

        public static IVsDifferenceService DifferenceService => GetGlobalService<SVsDifferenceService, IVsDifferenceService>();

        public static IVsActivityLog GetActivityLog(this IServiceProvider provider)
        {
            return GetGlobalService<SVsActivityLog, IVsActivityLog>(provider);
        }

        public static IVsSolution GetSolution(this IServiceProvider provider)
        {
            return GetGlobalService<SVsSolution, IVsSolution>(provider);
        }

        /// <summary>
        /// Gets the file name of the currently active document in the text editor, 
        /// or null if there there is no active document.
        /// </summary>
        public static string GetFileNameFromActiveDocument()
        {
            var fullName = Dte2?.ActiveDocument?.FullName;
            return Path.GetFileName(fullName);
        }
    }
}
