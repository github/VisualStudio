using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GitHub.Logging;
using GitHub.VisualStudio;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Setup.Configuration;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Rothko;
using Serilog;
using DTE = EnvDTE.DTE;

namespace GitHub.Services
{
    [Export(typeof(IVSServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VSServices : IVSServices
    {
        static readonly ILogger log = LogManager.ForContext<VSServices>();
        readonly IGitHubServiceProvider serviceProvider;

        // Use a prefix (~$) that is defined in the default VS gitignore.
        public const string TempSolutionName = "~$GitHubVSTemp$~";


        [ImportingConstructor]
        public VSServices(IGitHubServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        string vsVersion;
        public string VSVersion
        {
            get
            {
                if (vsVersion == null)
                    vsVersion = GetVSVersion();
                return vsVersion;
            }
        }


        public void ActivityLogMessage(string message)
        {
            var log = serviceProvider.GetActivityLog();
            if (log != null)
            {
                if (!ErrorHandler.Succeeded(log.LogEntry((UInt32)__ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION,
                            Info.ApplicationInfo.ApplicationSafeName, message)))
                    Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "Could not log message to activity log: {0}", message));
            }
        }

        public void ActivityLogError(string message)
        {
            var log = serviceProvider.GetActivityLog();
            if (log != null)
            {

                if (!ErrorHandler.Succeeded(log.LogEntry((UInt32)__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR,
                            Info.ApplicationInfo.ApplicationSafeName, message)))
                    Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "Could not log error to activity log: {0}", message));
            }
        }

        public void ActivityLogWarning(string message)
        {
            var log = serviceProvider.GetActivityLog();
            if (log != null)
            {
                if (!ErrorHandler.Succeeded(log.LogEntry((UInt32)__ACTIVITYLOG_ENTRYTYPE.ALE_WARNING,
                            Info.ApplicationInfo.ApplicationSafeName, message)))
                    Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "Could not log warning to activity log: {0}", message));
            }
        }

        /// <summary>Open a repository in Team Explorer</summary>
        /// <remarks>
        /// There doesn't appear to be a command that directly opens a target repo.
        /// Our workaround is to create, open and delete a solution in the repo directory.
        /// This triggers an event that causes the target repo to open. ;)
        /// </remarks>
        /// <param name="repoPath">The path to the repository to open</param>
        /// <returns>True if a transient solution was successfully created in target directory (which should trigger opening of repository).</returns>
        public bool TryOpenRepository(string repoPath)
        {
            var os = serviceProvider.TryGetService<IOperatingSystem>();
            if (os == null)
            {
                log.Error("TryOpenRepository couldn't find IOperatingSystem service");
                return false;
            }

            var dte = serviceProvider.TryGetService<DTE>();
            if (dte == null)
            {
                log.Error("TryOpenRepository couldn't find DTE service");
                return false;
            }

            var repoDir = os.Directory.GetDirectory(repoPath);
            if(!repoDir.Exists)
            {
                return false;
            }

            bool solutionCreated = false;
            try
            {
                dte.Solution.Create(repoPath, TempSolutionName);
                solutionCreated = true;

                dte.Solution.Close(false); // Don't create a .sln file when we close.
            }
            catch (Exception e)
            {
                log.Error(e, "Error opening repository");
            }
            finally
            {
                TryCleanupSolutionUserFiles(os, repoPath, TempSolutionName);
            }
            return solutionCreated;
        }

        void TryCleanupSolutionUserFiles(IOperatingSystem os, string repoPath, string slnName)
        {
            var vsTempPath = Path.Combine(repoPath, ".vs", slnName);
            try
            {
                // Clean up the dummy solution's subdirectory inside `.vs`.
                var vsTempDir = os.Directory.GetDirectory(vsTempPath);
                if (vsTempDir.Exists)
                {
                    vsTempDir.Delete(true);
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Couldn't clean up {TempPath}", vsTempPath);
            }
        }

        const string RegistryRootKey = @"Software\Microsoft\VisualStudio";
        const string EnvVersionKey = "EnvVersion";
        const string InstallationNamePrefix = "VisualStudio/";
        string GetVSVersion()
        {
            var version = typeof(Microsoft.VisualStudio.Shell.ActivityLog).Assembly.GetName().Version;
            var keyPath = String.Format(CultureInfo.InvariantCulture, "{0}\\{1}.{2}_Config\\SplashInfo", RegistryRootKey, version.Major, version.Minor);
            try
            {
                if (version.Major == 14)
                {
                    using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyPath))
                    {
                        var value = (string)key.GetValue(EnvVersionKey, String.Empty);
                        if (!String.IsNullOrEmpty(value))
                            return value;
                    }
                }
                else
                {
                    var setupConfiguration = new SetupConfiguration();
                    var setupInstance = setupConfiguration.GetInstanceForCurrentProcess();
                    return setupInstance.GetInstallationName().TrimPrefix(InstallationNamePrefix);
                }
            }
            catch(Exception ex)
            {
                log.Error(ex, "Error getting the Visual Studio version");
            }
            return version.ToString();
        }
    }
}
