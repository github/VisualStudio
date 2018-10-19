using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using GitHub.Logging;
using Serilog;

namespace GitHub.Helpers
{
    public class BindingPathUtilities
    {
        static readonly ILogger log = LogManager.ForContext<BindingPathUtilities>();

        public static void RationalizeBindingPaths(string assemblyLocation, List<string> bindingPaths = null)
        {
            bindingPaths = bindingPaths ?? FindBindingPaths();

            var fileName = Path.GetFileName(assemblyLocation);
            bindingPaths
                .Select(p => new { path = p, file = Path.Combine(p, fileName) })
                .Where(pf => File.Exists(pf.file))
                .Where(pf => !pf.file.Equals(assemblyLocation, StringComparison.OrdinalIgnoreCase))
                .ToList()
                .ForEach(pf =>
                {
                    var loaded = IsAssemblyLoaded(pf.file);
                    if (loaded)
                    {
                        log.Error("Assembly has already been loaded from {Location}", pf.file);
                    }

                    log.Warning("Removing duplicate binding path {BindingPath}", pf.path);
                    bindingPaths.Remove(pf.path);
                });
        }

        public static bool IsAssemblyLoaded(string assemblyLocation)
        {
            var prefix = Path.GetFileNameWithoutExtension(assemblyLocation) + ",";
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Where(a => a.Location.Equals(assemblyLocation, StringComparison.OrdinalIgnoreCase))
                .Any();
        }

        public static List<string> FindBindingPaths()
        {
            var manager = AppDomain.CurrentDomain.DomainManager;
            var property = manager.GetType().GetProperty("BindingPaths", BindingFlags.NonPublic | BindingFlags.Instance);
            return (List<string>)property?.GetValue(manager);
        }
    }
}
