using System;
using System.Runtime.InteropServices;
using GitHub.VisualStudio;
using Microsoft.VisualStudio.Shell;
using ICodeContainerProvider = Microsoft.VisualStudio.Shell.CodeContainerManagement.ICodeContainerProvider;

namespace GitHub.StartPage
{
    // Allow assemblies in the UI directory to be resolved by their full or partial name.
    // This is required for .imagemanifest files, XAML and when using unsigned assemblies.
    // See: https://github.com/github/VisualStudio/pull/1236/
    [ProvideBindingPath(SubPath = "UI")]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(Guids.StartPagePackageId)]
    [ProvideCodeContainerProvider("GitHub Container", Guids.StartPagePackageId, Images.ImageMonikerGuid, Images.LogoId, "#110", "#111", typeof(InBoxGitHubContainerProvider))]
    public sealed class StartPagePackage : ExtensionPointPackage
    {
        protected override ICodeContainerProvider CreateCodeContainerProvider(Guid provider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var codeContainerProvider = FullExtensionUtilities.FindGitHubContainerProvider(this);
            if (codeContainerProvider != null)
            {
                return codeContainerProvider;
            }

            return new InBoxGitHubContainerProvider();
        }
    }
}
