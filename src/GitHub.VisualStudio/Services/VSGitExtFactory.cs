extern alias TF14;
extern alias TF15;

using System;
using GitHub.Logging;
using Serilog;
using Microsoft.VisualStudio.Shell;
using VSGitExt14 = TF14.GitHub.VisualStudio.Base.VSGitExt;
using VSGitExt15 = TF15.GitHub.VisualStudio.Base.VSGitExt;

namespace GitHub.Services
{
    public class VSGitExtFactory
    {
        static readonly ILogger log = LogManager.ForContext<VSGitExtFactory>();

        readonly int vsVersion;
        readonly IAsyncServiceProvider asyncServiceProvider;

        public VSGitExtFactory(int vsVersion, IAsyncServiceProvider asyncServiceProvider)
        {
            this.vsVersion = vsVersion;
            this.asyncServiceProvider = asyncServiceProvider;
        }

        public IVSGitExt Create()
        {
            switch (vsVersion)
            {
                case 14:
                    return Create(() => new VSGitExt14(asyncServiceProvider));
                case 15:
                case 16:
                    return Create(() => new VSGitExt15(asyncServiceProvider));
                default:
                    log.Error("There is no IVSGitExt implementation for DTE version {Version}", vsVersion);
                    return null;
            }
        }

        // NOTE: We're being careful to only reference VSGitExt14/VSGitExt15/VSGitExt16 from inside a lambda expression.
        // This ensures that only the type that's compatible with the running DTE version is loaded.
        static IVSGitExt Create(Func<IVSGitExt> factory) => factory.Invoke();
    }
}
