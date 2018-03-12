extern alias TF14;
extern alias TF15;

using System;
using System.ComponentModel.Composition;
using GitHub.Info;
using GitHub.Logging;
using Serilog;
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

        public static IVSGitExt Create(int vsVersion, IAsyncServiceProvider sp)
        {
            switch (vsVersion)
            {
                case 14:
                    return Create(() => new VSGitExt14(sp));
                case 15:
                    return Create(() => new VSGitExt15(sp));
                default:
                    log.Error("There is no IVSGitExt implementation for DTE version {Version}", vsVersion);
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
