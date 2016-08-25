using GitHub.VisualStudio;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using System.Diagnostics;

namespace GitHub.Helpers
{
    public static class AssemblyResolver
    {
        static bool resolverInitialized;

        // list of assemblies to be loaded from the extension installation path
        static readonly string[] ourAssemblies =
        {
            "GitHub.Api",
            "GitHub.App",
            "GitHub.CredentialManagement",
            "GitHub.Exports",
            "GitHub.Exports.Reactive",
            "GitHub.Extensions",
            "GitHub.Extensions.Reactive",
            "GitHub.UI",
            "GitHub.UI.Reactive",
            "GitHub.VisualStudio",
            "GitHub.TeamFoundation",
            "GitHub.TeamFoundation.14",
            "GitHub.TeamFoundation.15",
            "GitHub.VisualStudio.UI",
            "System.Windows.Interactivity"
        };

        public static void InitializeAssemblyResolver()
        {
            if (resolverInitialized)
                return;
            AppDomain.CurrentDomain.AssemblyResolve += LoadAssemblyFromRunDir;
            resolverInitialized = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]
        static Assembly LoadAssemblyFromRunDir(object sender, ResolveEventArgs e)
        {
            try
            {
                var requestedName = e.Name.TrimSuffix(".dll", StringComparison.OrdinalIgnoreCase);
                var name = new AssemblyName(requestedName).Name;
                if (!ourAssemblies.Contains(name))
                    return null;
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var filename = Path.Combine(path, name + ".dll");
                if (!File.Exists(filename))
                    return null;

                return Assembly.LoadFrom(filename);
            }
            catch (Exception ex)
            {
                var log = string.Format(CultureInfo.CurrentCulture,
                    "Error occurred loading {0} from {1}.{2}{3}{4}",
                    e.Name,
                    Assembly.GetExecutingAssembly().Location,
                    Environment.NewLine,
                    ex,
                    Environment.NewLine);
                VsOutputLogger.Write(log);
            }
            return null;
        }
    }
}
