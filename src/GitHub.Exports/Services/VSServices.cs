using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using GitHub.Logging;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Setup.Configuration;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Rothko;
using Serilog;

namespace GitHub.Services
{
    [Export(typeof(IVSServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VSServices : IVSServices
    {
        readonly static ILogger log = LogManager.ForContext<VSServices>();
        readonly IGitHubServiceProvider serviceProvider;

        [ImportingConstructor]
        public VSServices(IGitHubServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        string vsVersion;
        /// <inheritdoc/>
        public string VSVersion
        {
            get
            {
                if (vsVersion == null)
                    vsVersion = GetVSVersion();
                return vsVersion;
            }
        }

        /// <inheritdoc/>
        public VSConstants.MessageBoxResult ShowMessageBoxInfo(string message)
        {
            return (VSConstants.MessageBoxResult)VsShellUtilities.ShowMessageBox(serviceProvider, message, null,
                OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
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
            catch (Exception ex)
            {
                log.Error(ex, "Error getting the Visual Studio version");
            }
            return version.ToString();
        }
    }
}
