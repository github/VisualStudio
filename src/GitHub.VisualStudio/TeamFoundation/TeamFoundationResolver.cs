using System;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel.Composition;

namespace GitHub.TeamFoundation
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamFoundationResolver : IDisposable
    {
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
            var name = args.Name;
            if (name.StartsWith("Microsoft.TeamFoundation."))
            {
                var assemblyName = new AssemblyName(name);
                try
                {
                    return Assembly.Load(assemblyName.Name);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                }
            }

            return null;
        }
    }
}
