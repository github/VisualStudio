using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using GitHub.Helpers;
using GitHub.Logging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace GitHub.VisualStudio.Helpers
{
    class BindingPathHelper
    {
        static readonly ILogger log = LogManager.ForContext<BindingPathHelper>();

        internal async static Task RationalizeBindingPathsAsync(
            Assembly assembly,
            JoinableTaskFactory jtf,
            IServiceProvider serviceProvider)
        {
            log.Information("Searching for duplicate binding paths");
            var assemblyLocation = assembly.Location;
            var bindingPaths = BindingPathUtilities.FindBindingPaths();
            var redundantPaths = BindingPathUtilities.FindRedundantBindingPaths(bindingPaths, assemblyLocation);
            if (redundantPaths.Count == 0)
            {
                log.Information("No duplicate binding paths found");
                return;
            }

            // Log what has been detected
            foreach (var redundantPath in redundantPaths)
            {
                log.Warning("Found redundant binding path {BindingPath}", redundantPath);

                var redundantFile = Path.Combine(redundantPath, Path.GetFileName(assemblyLocation));
                var loaded = BindingPathUtilities.IsAssemblyLoaded(redundantFile);
                if (loaded)
                {
                    log.Error("Assembly has already been loaded from {Location}", redundantFile);
                }
            }

            await jtf.SwitchToMainThreadAsync();
            var message = string.Format(@"Redundant binding path found at:
{0}

Would like to:
[Abort]  - Learn more about this issue
[Retry]  - Attempt a fix (this might not work)
[Ignore] - Don't do anything
", redundantPaths.First());
            var action = VsShellUtilities.ShowMessageBox(serviceProvider, message, "Redundant Binding Path", OLEMSGICON.OLEMSGICON_WARNING,
                OLEMSGBUTTON.OLEMSGBUTTON_ABORTRETRYIGNORE, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND);
            switch (action)
            {
                case 3: // Abort
                    Process.Start("https://github.com/github/VisualStudio/issues/1995");
                    break;
                case 4: // Retry - Try to fix
                    BindingPathUtilities.RemoveRedundantBindingPaths(bindingPaths, assemblyLocation, redundantPaths);
                    break;
                case 5: // Ignore
                    break;
            }
        }
    }
}
