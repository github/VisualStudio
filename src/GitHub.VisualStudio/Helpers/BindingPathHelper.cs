using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using GitHub.Helpers;
using GitHub.Logging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Serilog;

namespace GitHub.VisualStudio.Helpers
{
    /// <summary>
    // When running in the Exp instance, ensure there is only one active binding path.
    // This is necessary when the regular (AllUsers) extension is also installed.
    /// </summary>
    /// <remarks>
    /// See https://github.com/github/VisualStudio/issues/1995
    /// </remarks>
    class BindingPathHelper
    {
        static readonly ILogger log = LogManager.ForContext<BindingPathHelper>();

        internal static void CheckBindingPaths(Assembly assembly, IServiceProvider serviceProvider)
        {
            log.Information("Looking for assembly on wrong binding path");

            ThreadHelper.CheckAccess();
            var bindingPaths = BindingPathUtilities.FindBindingPaths(serviceProvider);
            var bindingPath = BindingPathUtilities.FindRedundantBindingPaths(bindingPaths, assembly.Location)
                .FirstOrDefault();
            if (bindingPath == null)
            {
                log.Information("No incorrect binding path found");
                return;
            }

            // Log what has been detected
            log.Warning("Found assembly on wrong binding path {BindingPath}", bindingPath);

            var message = string.Format(@"Found assembly on wrong binding path:
{0}

Would you like to learn more about this issue?", bindingPath);
            var action = VsShellUtilities.ShowMessageBox(serviceProvider, message, "GitHub for Visual Studio", OLEMSGICON.OLEMSGICON_WARNING,
                OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            if (action == 6) // Yes = 6, No = 7
            {
                Process.Start("https://github.com/github/VisualStudio/issues/2006");
            }
        }
    }
}
