using System;
using System.ComponentModel.Composition;

namespace GitHub.VisualStudio.TeamFoundation
{
    /// <summary>
    /// This should be imported by any MEF export factory that creates objects
    /// that reference the `Microsoft.TeamFoundation.*` assemblies.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class TeamFoundationResolverAttribute : ImportAttribute
    {
        public TeamFoundationResolverAttribute() : base(TeamFoundationResolver.ContractName, typeof(IResolver))
        {
        }
    }

    [Export(ContractName, typeof(IResolver))]
    public class TeamFoundationResolver : PriorityAssemblyResolver
    {
        public const string ContractName = "TeamFoundation";

        public TeamFoundationResolver() : base(
            @"CommonExtensions\Microsoft\TeamFoundation\Team Explorer",
            "Microsoft.TeamFoundation.", ", PublicKeyToken=b03f5f7f11d50a3a")
        {
        }

        public static Type Resolve(Func<Type> func)
        {
            using (new TeamFoundationResolver())
            {
                return func();
            }
        }
    }
}
