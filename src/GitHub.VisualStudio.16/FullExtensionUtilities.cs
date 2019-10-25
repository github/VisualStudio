using System;
using System.IO;
using System.Reflection;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ICodeContainerProvider = Microsoft.VisualStudio.Shell.CodeContainerManagement.ICodeContainerProvider;

namespace GitHub.VisualStudio
{
    public static class FullExtensionUtilities
    {
        const string GitHubPackageId = "c3d3dc68-c977-411f-b3e8-03b0dccf7dfc";

        public static bool IsInstalled(IServiceProvider serviceProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            return FindGitHubPackage(serviceProvider) != null;
        }

        public static ICodeContainerProvider FindGitHubContainerProvider(IServiceProvider serviceProvide)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (FindGitHubPackage(serviceProvide) is IVsPackage package)
            {
                var baseDirectory = Path.GetDirectoryName(package.GetType().Assembly.Location);
                var assemblyFile = Path.Combine(baseDirectory, "GitHub.StartPage.dll");
                var assembly = Assembly.LoadFrom(assemblyFile);
                var type = assembly.GetType("GitHub.StartPage.GitHubContainerProvider", true);
                return (ICodeContainerProvider)Activator.CreateInstance(type);
            }

            return null;
        }

        public static IVsPackage FindGitHubPackage(IServiceProvider serviceProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var shell = serviceProvider.GetService(typeof(SVsShell)) as IVsShell;
            Assumes.Present(shell);
            shell.LoadPackage(new Guid(GitHubPackageId), out var package);
            return package;
        }
    }
}
