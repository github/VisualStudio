extern alias TF14;
extern alias TF15;
extern alias TF16;

using System;
using GitHub.Logging;
using Serilog;
using VSGitExt14 = TF14.GitHub.VisualStudio.Base.VSGitExt;
using VSGitExt15 = TF15.GitHub.VisualStudio.Base.VSGitExt;
using VSGitExt16 = TF16.GitHub.VisualStudio.Base.VSGitExt;

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

        public IVSGitExt Create()
        {
            switch (vsVersion)
            {
                case 14:
                    return Create(() => new VSGitExt14(serviceProvider, gitService));
                case 15:
                    return Create(() => new VSGitExt15(serviceProvider, gitService));
                case 16:
                    return Create(() => new VSGitExt16(serviceProvider, gitService));
                default:
                    log.Error("There is no IVSGitExt implementation for DTE version {Version}", vsVersion);
                    return null;
            }
        }

        // NOTE: We're being careful to only reference VSGitExt14 and VSGitExt15 from inside a lambda expression.
        // This ensures that only the type that's compatible with the running DTE version is loaded.
        static IVSGitExt Create(Func<IVSGitExt> factory) => factory.Invoke();
    }
}
