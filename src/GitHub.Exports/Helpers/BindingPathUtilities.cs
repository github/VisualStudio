using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using GitHub.Logging;
using Serilog;

namespace GitHub.Helpers
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
    /// This is a workaround for that issue. The <see cref="RationalizeBindingPaths( List{string},string)"/>
    /// method will check to see if a reference assembly could be loaded from an alternative
    /// binding path. It will remove any alternative paths that is finds so that XAML or
    /// .imagemanifest won't end up loading the wrong assembly.
    /// </remarks>
    public static class BindingPathUtilities
    {
        static readonly ILogger log = LogManager.ForContext(typeof(BindingPathUtilities));

        /// <summary>
        /// Remove alternative binding path that might have been installed by an AllUsers extension.
        /// </summary>
        /// <param name="bindingPaths">A list of binding paths to rationalize</param>
        /// <param name="assemblyLocation">A reference assembly that has been loaded from the correct path.</param>
        /// <returns>True if binding path was removed.</returns>
        public static bool RationalizeBindingPaths(List<string> bindingPaths, string assemblyLocation)
        {
            var redundantBindingPaths = FindRedundantBindingPaths(bindingPaths, assemblyLocation);
            RemoveRedundantBindingPaths(bindingPaths, assemblyLocation, redundantBindingPaths);
            return redundantBindingPaths.Any();
        }

        /// <summary>
        /// Find any alternative binding path that might have been installed by an AllUsers extension.
        /// </summary>
        /// <param name="bindingPaths">A list of binding paths to search</param>
        /// <param name="assemblyLocation">A reference assembly that has been loaded from the correct path.</param>
        /// <returns>A list of redundant binding paths.</returns>
        public static IList<string> FindRedundantBindingPaths(List<string> bindingPaths, string assemblyLocation)
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
        /// Check to see if an assembly is already in memory.
        /// </summary>
        /// <param name="assemblyLocation">The assembly to look for.</param>
        /// <returns>True if assembly has already been loaded.</returns>
        public static bool IsAssemblyLoaded(string assemblyLocation)
        {
            var prefix = Path.GetFileNameWithoutExtension(assemblyLocation) + ",";
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Where(a => a.Location.Equals(assemblyLocation, StringComparison.OrdinalIgnoreCase))
                .Any();
        }

        /// <summary>
        /// Use reflection to find Visual Studio's list of binding paths.
        /// </summary>
        /// <returns>A live list of binding paths or an empty list if not running in Visual Studio.</returns>
        public static List<string> FindBindingPaths()
        {
            var manager = AppDomain.CurrentDomain.DomainManager;
            var property = manager?.GetType().GetProperty("BindingPaths", BindingFlags.NonPublic | BindingFlags.Instance);
            var bindingPaths = property?.GetValue(manager) as List<string>;
            return bindingPaths ?? new List<string>(0);
        }

        public static void RemoveRedundantBindingPaths(List<string> bindingPaths, string assemblyLocation,
            IList<string> redundantBindingPaths)
        {
            redundantBindingPaths
                .ForEach(p => bindingPaths.Remove(p));
        }
    }
}
