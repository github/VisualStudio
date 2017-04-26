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
        const string AssemblyPrefix = "Microsoft.TeamFoundation.";

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
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
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
                if (name.StartsWith(AssemblyPrefix))
                {
                    var assemblyName = new AssemblyName(name);
                    var path = GetTeamExplorerPath(assemblyName.Name);
                    if(File.Exists(path))
                    {
                        return Assembly.LoadFrom(path);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }

            return null;
        }

        static string GetTeamExplorerPath(string name)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BindingPath, name + ".dll");
        }
    }
}
