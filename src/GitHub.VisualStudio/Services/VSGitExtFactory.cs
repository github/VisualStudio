extern alias TF14;
extern alias TF15;

using System;
using GitHub.Logging;
using Serilog;
using VSGitExt14 = TF14.GitHub.VisualStudio.Base.VSGitExt;
using VSGitExt15 = TF15.GitHub.VisualStudio.Base.VSGitExt;

namespace GitHub.Services
{
    public class VSGitExtFactory
    {
        static readonly ILogger log = LogManager.ForContext<VSGitExtFactory>();

        readonly int vsVersion;
        readonly IServiceProvider serviceProvider;

        public VSGitExtFactory(int vsVersion, IServiceProvider serviceProvider)
        {
            this.vsVersion = vsVersion;
            this.serviceProvider = serviceProvider;
        }

        public IVSGitExt Create()
        {
            switch (vsVersion)
            {
                case 14:
                    return Create(() => new VSGitExt14(serviceProvider));
                case 15:
                case 16:
                    return Create(() => new VSGitExt15(serviceProvider));
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
