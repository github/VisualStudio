using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ComponentModelHost;

namespace GitHub.VisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(Guids.InitializeMefPackageId)]
    [ProvideAutoLoad(Guids.GitSccProviderId)]
    public sealed class InitializeMefPackage : Package
    {
        /// <summary>
        /// Ensure that MEF is initialized using an old style non-async package.
        /// This causes the Scanning new and updated MEF components... dialog to appear.
        /// Without this the GitHub pane will be blamed for degrading startup performance.
        /// </summary>
        protected override void Initialize()
        {
            GetService(typeof(SComponentModel));
        }
    }
}
