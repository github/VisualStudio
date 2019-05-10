using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;
using ICodeContainerProvider = Microsoft.VisualStudio.Shell.CodeContainerManagement.ICodeContainerProvider;

namespace GitHub.VisualStudio
{
    public class InstalledExtensionServices : IExtensionServices
    {
        readonly IVsPackage package;

        public InstalledExtensionServices(IVsPackage package)
        {
            this.package = package;
        }

        public ICodeContainerProvider GetGitHubContainerProvider()
        {
            var baseDirectory = Path.GetDirectoryName(package.GetType().Assembly.Location);
            var assemblyFile = Path.Combine(baseDirectory, "GitHub.StartPage.dll");
            var assembly = Assembly.LoadFrom(assemblyFile);
            var type = assembly.GetType("GitHub.StartPage.GitHubContainerProvider", true);
            return (ICodeContainerProvider)Activator.CreateInstance(type);
        }

        public Task LoginAsync()
        {
            throw new NotImplementedException();
        }
    }
}
