using System;
using System.Reflection;
using GitHub.Logging;
using GitHub.VisualStudio.Base;
using Serilog;

namespace GitHub.Services
{
    public class VSGitExtFactory
    {
        static readonly ILogger log = LogManager.ForContext<VSGitExtFactory>();

        readonly int vsVersion;
        readonly IServiceProvider serviceProvider;
        readonly IGitService gitService;

        public VSGitExtFactory(int vsVersion, IServiceProvider serviceProvider, IGitService gitService)
        {
            this.vsVersion = vsVersion;
            this.serviceProvider = serviceProvider;
            this.gitService = gitService;
        }

        // The GitHub.TeamFoundation.* assemblies target different .NET and Visual Studio versions.
        // We can't reference all of their projects directly, so instead we use reflection to retrieve
        // and instantiate the correct implementation.
        public IVSGitExt Create()
        {
            if(Type.GetType($"GitHub.VisualStudio.Base.VSGitExt, GitHub.TeamFoundation.{vsVersion}", false) is Type type)
            {
                return (IVSGitExt)Activator.CreateInstance(type, serviceProvider, gitService);
            }

            log.Error("There is no IVSGitExt implementation for DTE version {Version}", vsVersion);
            return null;
        }
    }
}
