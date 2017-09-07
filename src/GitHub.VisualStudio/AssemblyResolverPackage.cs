using System;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using GitHub.Services;

namespace GitHub.VisualStudio
{
    // This is the Git service GUID, which fires early and is used by GitHubService.
    [ProvideAutoLoad(Guids.GitSccProviderId)]
    // This fires before ShellInitialized and SolutionExists.
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)]
    [Guid(Guids.guidAssemblyResolverPkgString)]
    public class AssemblyResolverPackage : Package
    {
        // list of assemblies that should be considered when resolving
        IEnumerable<ProvideDependentAssemblyAttribute> dependentAssemblies;

        string packageFolder;

        protected override void Initialize()
        {
            var asm = Assembly.GetExecutingAssembly();
            packageFolder = Path.GetDirectoryName(asm.Location);
            dependentAssemblies = asm.GetCustomAttributes<ProvideDependentAssemblyAttribute>();

            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblyFromPackageFolder;
        }

        protected override void Dispose(bool disposing)
        {
            AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssemblyFromPackageFolder;
            base.Dispose(disposing);
        }

        Assembly ResolveAssemblyFromPackageFolder(object sender, ResolveEventArgs e)
        {
            Assembly resolvedAssembly = null;

            try
            {
                var resolveAssemblyName = new AssemblyName(e.Name);
                foreach (var dependentAssembly in dependentAssemblies)
                {
                    resolvedAssembly = ResolveDependentAssembly(dependentAssembly, packageFolder, resolveAssemblyName);
                    if (resolvedAssembly != null)
                    {
                        var usage = (IUsageTracker)GetService(typeof(IUsageTracker));
                        if (resolveAssemblyName.KeyPair != null)
                        {
                            usage?.IncrementCounter(x => x.NumberOfSignedAssemblyResolves);
                        }
                        else
                        {
                            usage?.IncrementCounter(x => x.NumberOfUnsignedAssemblyResolves);
                        }

                        break;
                    }
                }
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

            return resolvedAssembly;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]
        public static Assembly ResolveDependentAssembly(
            ProvideDependentAssemblyAttribute dependentAssembly, string packageFolder, AssemblyName resolveAssemblyName)
        {
            if (dependentAssembly.AssemblyName == resolveAssemblyName.Name)
            {
                var file = dependentAssembly.CodeBase.Replace("$PackageFolder$", packageFolder);
                if (File.Exists(file))
                {
                    var targetAssemblyName = AssemblyName.GetAssemblyName(file);

                    var codeBase = dependentAssembly as ProvideCodeBaseAttribute;
                    if (codeBase != null)
                    {
                        if (resolveAssemblyName.FullName == targetAssemblyName.FullName)
                        {
                            return Assembly.LoadFrom(file);
                        }
                    }

                    var bindingRedirection = dependentAssembly as ProvideBindingRedirectionAttribute;
                    if (bindingRedirection != null)
                    {
                        if (resolveAssemblyName.Version >= new Version(bindingRedirection.OldVersionLowerBound) &&
                            resolveAssemblyName.Version <= new Version(bindingRedirection.OldVersionUpperBound))
                        {
                            resolveAssemblyName.Version = targetAssemblyName.Version;
                            if (resolveAssemblyName.FullName == targetAssemblyName.FullName)
                            {
                                return Assembly.LoadFrom(file);
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
