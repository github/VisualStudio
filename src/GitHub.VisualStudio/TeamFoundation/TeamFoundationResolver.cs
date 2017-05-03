using System;
using System.IO;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.Composition;

namespace GitHub.VisualStudio.TeamFoundation
{
    [Export(typeof(ITeamFoundationResolver))]
    public class TeamFoundationResolver : ITeamFoundationResolver, IDisposable
    {
        bool disposed;

        readonly string bindingPath;
        readonly string assemblyStartsWith;
        readonly string assemblyEndsWith;

        public TeamFoundationResolver() : this(@"CommonExtensions\Microsoft\TeamFoundation\Team Explorer",
            "Microsoft.TeamFoundation.", ", PublicKeyToken=b03f5f7f11d50a3a")
        {
            TryAddPriorityAssemblyResolve(AppDomain.CurrentDomain, CurrentDomain_AssemblyResolve);
        }

        public TeamFoundationResolver(string bindingPath, string assemblyStartsWith, string assemblyEndsWith)
        {
            this.bindingPath = bindingPath;
            this.assemblyStartsWith = assemblyStartsWith;
            this.assemblyEndsWith = assemblyEndsWith;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
                disposed = true;
            }
        }

        public static Type Resolve(Func<Type> func)
        {
            using (new TeamFoundationResolver())
            {
                return func();
            }
        }

        // NOTE: This is a workaround for https://github.com/github/VisualStudio/issues/923#issuecomment-287537118
        // A consistent repro was to open Visual Studio 2015 by double clicking on GitHubVS.sln.
        // The `Microsoft.TeamFoundation.Controls.dll` assembly referenced by this solution would
        // be copied to and loaded from the following location:
        // C:\Users\<user>\AppData\Local\Microsoft\VisualStudio\14.0\ProjectAssemblies\ffp8wnnz01\Microsoft.TeamFoundation.Controls.dll
        //
        // This method ensures that our resolver has a chance to resolve it first.
        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "I like $ strings")]
        static void TryAddPriorityAssemblyResolve(AppDomain domain, ResolveEventHandler handler)
        {
            try
            {
                var resolveField = typeof(AppDomain).GetField("_AssemblyResolve", BindingFlags.NonPublic | BindingFlags.Instance);
                var assemblyResolve = (ResolveEventHandler)resolveField.GetValue(domain);
                if (assemblyResolve != null)
                {
                    handler = (ResolveEventHandler)Delegate.Combine(handler, assemblyResolve);
                }

                resolveField.SetValue(domain, handler);
            }
            catch (Exception e)
            {
                VsOutputLogger.WriteLine($"Couldn't add priority AssemblyResolve handler (adding normal handler): {e}");
                domain.AssemblyResolve += handler;
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods",
            Justification = "Assembly.LoadFrom is normal for AssemblyResolve event")]
        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "I like $ strings")]
        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var name = args.Name;
                if (name.StartsWith(assemblyStartsWith, StringComparison.Ordinal) &&
                    name.EndsWith(assemblyEndsWith, StringComparison.Ordinal))
                {
                    var assemblyName = new AssemblyName(name);
                    var path = GetTeamExplorerPath(assemblyName.Name);
                    if (File.Exists(path))
                    {
                        return Assembly.LoadFrom(path);
                    }

                    VsOutputLogger.WriteLine($"Couldn't find TeamFoundation assembly '{args.Name}' at {path}.");
                }
            }
            catch (Exception e)
            {
                VsOutputLogger.WriteLine($"Couldn't resolve TeamFoundation assembly '{args.Name}': {e}");
            }

            return null;
        }

        string GetTeamExplorerPath(string name)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, bindingPath, name + ".dll");
        }
    }
}
