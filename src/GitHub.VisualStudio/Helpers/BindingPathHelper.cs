using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using GitHub.Logging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Settings;
using Serilog;

namespace GitHub.VisualStudio.Helpers
{
    /// <summary>
    /// This a workaround for extensions that define a ProvideBindingPath attribute and
    /// install for AllUsers.
    /// </summary>
    /// <remarks>
    /// Extensions that are installed for AllUsers, will also be installed for all
    /// instances of Visual Studio - including the experimental (Exp) instance which
    /// is used in development. This isn't a problem so long as all features that
    /// exist in the AllUsers extension, also exist in the extension that is being
    /// developed.
    /// 
    /// When an extension uses the ProvideBindingPath attribute, the binding path for
    /// the AllUsers extension gets installed at the same time as the one in development.
    /// This doesn't matter when an assembly is strong named and is loaded using its
    /// full name (including version number). When an assembly is loaded using its
    /// simple name, assemblies from the AllUsers extension can end up loaded at the
    /// same time as the extension being developed. This can happen when an assembly
    /// is loaded from XAML or an .imagemanifest.
    /// 
    /// This is a workaround for that issue. The <see cref="FindRedundantBindingPaths(List{string}, string)" />
    /// method will check to see if a reference assembly could be loaded from an alternative
    /// binding path. It will return any alternative paths that is finds.
    /// See https://github.com/github/VisualStudio/issues/1995
    /// </remarks>
    public static class BindingPathHelper
    {
        static readonly ILogger log = LogManager.ForContext(typeof(BindingPathHelper));

        internal static void CheckBindingPaths(Assembly assembly, IServiceProvider serviceProvider)
        {
            log.Information("Looking for assembly on wrong binding path");

            ThreadHelper.CheckAccess();
            var bindingPaths = FindBindingPaths(serviceProvider);
            var bindingPath = FindRedundantBindingPaths(bindingPaths, assembly.Location)
                .FirstOrDefault();
            if (bindingPath == null)
            {
                log.Information("No incorrect binding path found");
                return;
            }

            // Log what has been detected
            log.Warning("Found assembly on wrong binding path {BindingPath}", bindingPath);

            var message = string.Format(CultureInfo.CurrentCulture, @"Found assembly on wrong binding path:
{0}

Would you like to learn more about this issue?", bindingPath);
            var action = VsShellUtilities.ShowMessageBox(serviceProvider, message, "GitHub for Visual Studio", OLEMSGICON.OLEMSGICON_WARNING,
                OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            if (action == 6) // Yes = 6, No = 7
            {
                Process.Start("https://github.com/github/VisualStudio/issues/2006");
            }
        }

        /// <summary>
        /// Find any alternative binding path that might have been installed by an AllUsers extension.
        /// </summary>
        /// <param name="bindingPaths">A list of binding paths to search</param>
        /// <param name="assemblyLocation">A reference assembly that has been loaded from the correct path.</param>
        /// <returns>A list of redundant binding paths.</returns>
        public static IList<string> FindRedundantBindingPaths(IEnumerable<string> bindingPaths, string assemblyLocation)
        {
            var fileName = Path.GetFileName(assemblyLocation);
            return bindingPaths
                .Select(p => (path: p, file: Path.Combine(p, fileName)))
                .Where(pf => File.Exists(pf.file))
                .Where(pf => !pf.file.Equals(assemblyLocation, StringComparison.OrdinalIgnoreCase))
                .Select(pf => pf.path)
                .ToList();
        }

        /// <summary>
        /// Find Visual Studio's list of binding paths.
        /// </summary>
        /// <returns>A list of binding paths.</returns>
        public static IEnumerable<string> FindBindingPaths(IServiceProvider serviceProvider)
        {
            const string bindingPaths = "BindingPaths";
            var manager = new ShellSettingsManager(serviceProvider);
            var store = manager.GetReadOnlySettingsStore(SettingsScope.Configuration);
            foreach (var guid in store.GetSubCollectionNames(bindingPaths))
            {
                var guidPath = Path.Combine(bindingPaths, guid);
                foreach (var path in store.GetPropertyNames(guidPath))
                {
                    yield return path;
                }
            }
        }
    }
}
