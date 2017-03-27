using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using GitHub.Services;

namespace GitHub.VisualStudio
{
    [Guid(PackageGuidString)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideAppCommandLine("GitClone", typeof(GitCloneCommandLinePackage), Arguments = "1", DemandLoad = 1)]
    public sealed class GitCloneCommandLinePackage : Package
    {
        public const string PackageGuidString = "379dad24-c111-4d73-89ff-e8f91cab68db";

        protected override void Initialize()
        {
            try
            {
                var sp = (IGitHubServiceProvider)GetService(typeof(IGitHubServiceProvider));
                var gitCloneCommandLineService = sp.ExportProvider.GetExportedValue<GitCloneCommandLineService>();
                gitCloneCommandLineService.IfCloneOptionTryOpenRepository();
            }
            catch(Exception e)
            {
                Trace.WriteLine(e);
            }

            base.Initialize();
        }
    }
}
