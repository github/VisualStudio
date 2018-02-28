extern alias TF14;
extern alias TF15;

using System;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using GitHub.Logging;
using Serilog;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using VSGitExt14 = TF14.GitHub.VisualStudio.Base.VSGitExt;
using VSGitExt15 = TF15.GitHub.VisualStudio.Base.VSGitExt;

namespace GitHub.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VSGitExtFactory
    {
        static readonly ILogger log = LogManager.ForContext<VSGitExtFactory>();

        [ImportingConstructor]
        public VSGitExtFactory(IGitHubServiceProvider serviceProvider)
        {
            VSGitExt = serviceProvider.GetService<IVSGitExt>();
        }

        public async static Task<IVSGitExt> Create(IAsyncServiceProvider sp)
        {
            var dte = await sp.GetServiceAsync(typeof(DTE)) as DTE;

            // DTE.Version always ends with ".0" even for later minor versions.
            switch (dte.Version)
            {
                case "14.0":
                    return Create(() => new VSGitExt14(sp.GetServiceAsync));
                case "15.0":
                    return Create(() => new VSGitExt15(sp.GetServiceAsync));
                default:
                    log.Error("There is no IVSGitExt implementation for DTE version {Version}", dte.Version);
                    return null;
            }
        }

        // NOTE: We're being careful to only reference VSGitExt14 and VSGitExt15 from inside a lambda expression.
        // This ensures that only the type that's compatible with the running DTE version is loaded.
        static IVSGitExt Create(Func<IVSGitExt> factory) => factory.Invoke();

        [Export]
        public IVSGitExt VSGitExt { get; }
    }
}
