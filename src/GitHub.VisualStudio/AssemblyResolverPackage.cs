using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio
{
    // This is the Git service GUID, which fires early and is used by GitHubService.
    //[ProvideAutoLoad(Guids.GitSccProviderId)]

    // This fires before ShellInitialized and SolutionExists.
    //[ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)]

    [Guid(GuidList.guidAssemblyResolverPkgString)]
    public class AssemblyResolverPackage : Package
    {
        // list of assemblies that should be resolved by name only
        static readonly string[] ourAssemblies =
        {
            // resolver is required for these
            "GitHub.UI",
            "GitHub.VisualStudio.UI",

            // these are included just in case
            "GitHub.UI.Reactive",
            "System.Windows.Interactivity"
        };

        readonly string extensionDir;

        public AssemblyResolverPackage()
        {
            Debug.WriteLine("GitHub assembly resolver is now active.");
            extensionDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AppDomain.CurrentDomain.AssemblyResolve += LoadAssemblyFromExtensionDir;
        }

        protected override void Dispose(bool disposing)
        {
            AppDomain.CurrentDomain.AssemblyResolve -= LoadAssemblyFromExtensionDir;
            base.Dispose(disposing);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]
        Assembly LoadAssemblyFromExtensionDir(object sender, ResolveEventArgs e)
        {
            try
            {
                var name = new AssemblyName(e.Name).Name;
                var filename = Path.Combine(extensionDir, name + ".dll");
                if (!File.Exists(filename))
                {
                    return null;
                }

                var targetName = AssemblyName.GetAssemblyName(filename);

                // Resolve any exact `FullName` matches.
                if (e.Name != targetName.FullName)
                {
                    // Resolve any version of our assemblies.
                    if (!ourAssemblies.Contains(name, StringComparer.OrdinalIgnoreCase))
                    {
                        Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Not resolving '{0}' to '{1}'.", e.Name, targetName.FullName));
                        return null;
                    }

                    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Resolving '{0}' to '{1}'.", e.Name, targetName.FullName));
                }

                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Loading '{0}' from '{1}'.", targetName.FullName, filename));
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
                Trace.WriteLine(log);
                VsOutputLogger.Write(log);
            }

            return null;
        }
    }
}
