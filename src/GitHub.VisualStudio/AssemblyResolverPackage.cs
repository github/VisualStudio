using System;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using NLog;

namespace GitHub.VisualStudio
{
    // This fires before ShellInitialized and SolutionExists.
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)]
    [Guid(Guids.guidAssemblyResolverPkgString)]
    public class AssemblyResolverPackage : Package
    {
        // list of assemblies that should be considered when resolving
        IEnumerable<ProvideDependentAssemblyAttribute> dependentAssemblies;
        string packageFolder;

        IDictionary<string, Assembly> resolvingAssemblies;
        IDictionary<string, Exception> resolvingExceptions;

        public AssemblyResolverPackage()
        {
            var asm = Assembly.GetExecutingAssembly();
            packageFolder = Path.GetDirectoryName(asm.Location);
            dependentAssemblies = asm.GetCustomAttributes<ProvideDependentAssemblyAttribute>();
            resolvingAssemblies = new Dictionary<string, Assembly>();
            resolvingExceptions = new Dictionary<string, Exception>();
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblyFromPackageFolder;
        }

        protected override void Dispose(bool disposing)
        {
            AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssemblyFromPackageFolder;

            if (resolvingAssemblies.Count > 0 || resolvingExceptions.Count > 0)
            {
                // Avoid loading logging assembly unless there is something to log.
                WriteToLog();
            }

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
                        resolvingAssemblies[e.Name] = resolvedAssembly;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                resolvingExceptions[e.Name] = ex;
            }

            return resolvedAssembly;
        }

        void WriteToLog()
        {
            var log = LogManager.GetCurrentClassLogger();
            foreach (var resolvedAssembly in resolvingAssemblies)
            {
                log.Info(CultureInfo.InvariantCulture, "Resolved '{0}' to '{1}'.", resolvedAssembly.Key, resolvedAssembly.Value.Location);
            }

            foreach (var resolvingException in resolvingExceptions)
            {
                log.Error(CultureInfo.InvariantCulture, "Error occurred loading '{0}' from '{1}'.\n{2}", resolvingException.Key, packageFolder, resolvingException.Value);
            }
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
