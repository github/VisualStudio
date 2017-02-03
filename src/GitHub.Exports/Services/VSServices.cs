using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.ComponentModel.Composition;
using System.Globalization;
using GitHub.VisualStudio;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using DTE = EnvDTE.DTE;

namespace GitHub.Services
{
    [Export(typeof(IVSServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VSServices : IVSServices
    {
        readonly IGitHubServiceProvider serviceProvider;
        readonly DTE dte = null;

        [ImportingConstructor]
        public VSServices(IGitHubServiceProvider serviceProvider, SVsServiceProvider vsServiceProvider)
        {
            this.serviceProvider = serviceProvider;
            dte = (DTE)vsServiceProvider.GetService(typeof(DTE));
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

        /// <summary>
        /// There doesn't appear to be a command that directly opens a target repo.
        /// Our workaround is to create, open and delete a solution in the repo directory.
        /// This triggers an event that causes the target repo to open. ;)
        /// </summary>
        /// <param name="directory">The target repo directory. </param>
        public bool TryOpenRepository(string directory)
        {
            try
            {
                // Use a prefix (~$) that is defined in the default VS gitignore.
                var name = "~$GitHubVSTemp$~";

                // If .vs\ exists, create the dummy sln there.
                var vsDir = Path.Combine(directory, ".vs");
                if(Directory.Exists(vsDir))
                {
                    directory = vsDir;
                }

                var file = Path.Combine(directory, name + ".sln");

                // If something went wrong last time, the solution file might already exist.
                if(File.Exists(file))
                {
                    File.Delete(file);
                }

                dte.Solution.Create(directory, name);
                dte.Solution.Close(false);

                // Clean up the dummy solution file.
                // Try a couple of times incase antivirus app has take a lock.
                TryFileDelete(file, 2, 500);

                return true;
            }
            catch (Exception e)
            {
                VsOutputLogger.WriteLine("Error opening repository. {0}", e);
                return false;
            }
        }

        void TryFileDelete(string file, int retries, int msTimeout)
        {
            for(int count = 0; count < retries; count++)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    VsOutputLogger.WriteLine("Error deleting file. {0}", e);
                    Thread.Sleep(msTimeout);
                }
            }
        }

        const string RegistryRootKey = @"Software\Microsoft\VisualStudio";
        const string EnvVersionKey = "EnvVersion";
        string GetVSVersion()
        {
            var version = typeof(Microsoft.VisualStudio.Shell.ActivityLog).Assembly.GetName().Version;
            var keyPath = String.Format(CultureInfo.InvariantCulture, "{0}\\{1}.{2}_Config\\SplashInfo", RegistryRootKey, version.Major, version.Minor);
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyPath))
                {
                    var value = (string)key.GetValue(EnvVersionKey, String.Empty);
                    if (!String.IsNullOrEmpty(value))
                        return value;
                }
                // fallback to poking the CommonIDE assembly, which most closely follows the advertised version.
                var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.StartsWith("Microsoft.VisualStudio.CommonIDE", StringComparison.OrdinalIgnoreCase));
                if (asm != null)
                    return asm.GetName().Version.ToString();
            }
            catch(Exception ex)
            {
                VsOutputLogger.WriteLine(string.Format(CultureInfo.CurrentCulture, "Error getting the Visual Studio version '{0}'", ex));
            }
            return version.ToString();
        }
    }
}
