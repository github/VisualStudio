using System;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitHub.VisualStudio
{
    public class ExtensionServicesFactory
    {
        readonly IServiceProvider serviceProvider;

        public ExtensionServicesFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IExtensionServices Create()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var package = FindGitHubPackage();
            if(package != null)
            {
                return new InstalledExtensionServices(package);
            }
            else
            {
                return new InBoxExtensionServices();
            }
        }

        IVsPackage FindGitHubPackage()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var shell = serviceProvider.GetService(typeof(SVsShell)) as IVsShell;
            Assumes.Present(shell);
            shell.LoadPackage(new Guid(Guids.PackageId), out var package);
            return package;
        }
    }
}
