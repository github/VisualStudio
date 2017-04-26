using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel.Composition;

namespace GitHub.TeamFoundation
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamFoundationResolver : IDisposable
    {
        const string BindingPath = @"CommonExtensions\Microsoft\TeamFoundation\Team Explorer";
        const string AssemblyStartsWith = "Microsoft.TeamFoundation.";
        const string AssemblyEndsWith = ", PublicKeyToken=b03f5f7f11d50a3a";

        internal static Type Resolve(Func<Type> func)
        {
            using (new TeamFoundationResolver())
            {
                return func();
            }
        }

        internal static object Resolve(Func<object> func)
        {
            using (new TeamFoundationResolver())
            {
                return func();
            }
        }

        internal TeamFoundationResolver()
        {
            TryAddPriorityAssemblyResolve(AppDomain.CurrentDomain, CurrentDomain_AssemblyResolve);
        }

        // NOTE: This is a workaround for https://github.com/github/VisualStudio/issues/923#issuecomment-287537118
        // A consistent repro was to open Vsiaul Studio 2015 by double clicking on GitHubVS.sln.
        // The `Microsoft.TeamFoundation.Controls.dll` assembly referenced by this solution would
        // be copied to and loaded from the following location:
        // C:\Users\<user>\AppData\Local\Microsoft\VisualStudio\14.0\ProjectAssemblies\ffp8wnnz01\Microsoft.TeamFoundation.Controls.dll
        //
        // This method ensures that our resolver has a chance to resolve it first.
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
                Trace.WriteLine("Couldn't add priority AssemblyResolve handler (adding normal handler): " + e);
                domain.AssemblyResolve += handler;
            }
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var name = args.Name;
                if (name.StartsWith(AssemblyStartsWith) && name.EndsWith(AssemblyEndsWith))
                {
                    var assemblyName = new AssemblyName(name);
                    var path = GetTeamExplorerPath(assemblyName.Name);
                    if (File.Exists(path))
                    {
                        return Assembly.LoadFrom(path);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("Couldn't resolve TeamFoundation assembly: " + e);
            }

            return null;
        }

        static string GetTeamExplorerPath(string name)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BindingPath, name + ".dll");
        }
    }
}
