using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using GitHub.Extensions;
using Microsoft.Win32;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using GitHub.VisualStudio;
using Microsoft.TeamFoundation.Controls;

namespace GitHub.Services
{
    public interface IVSServices
    {
        string GetLocalClonePathFromGitProvider();
        void Clone(string cloneUrl, string clonePath, bool recurseSubmodules);
        string GetActiveRepoPath();
        LibGit2Sharp.Repository GetActiveRepo();

        void ShowMessage(string message);
        void ShowWarning(string message);
        void ShowError(string message);
    }

    [Export(typeof(IVSServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VSServices : IVSServices
    {
        readonly IServiceProvider serviceProvider;

        [ImportingConstructor]
        public VSServices(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        // The Default Repository Path that VS uses is hidden in an internal
        // service 'ISccSettingsService' registered in an internal service
        // 'ISccServiceHost' in an assembly with no public types that's
        // always loaded with VS if the git service provider is loaded
        public string GetLocalClonePathFromGitProvider()
        {
            string ret = string.Empty;
            try
            {
                var service = GitCoreServices.GetISccSettingsService(serviceProvider);
                ret = service.DefaultRepositoryPath;
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }

            if (!string.IsNullOrEmpty(ret))
                return ret;

            try
            {
                ret = PokeTheRegistryForLocalClonePath();
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }
            return ret;
        }

        public void Clone(string cloneUrl, string clonePath, bool recurseSubmodules)
        {
            var gitExt = serviceProvider.GetService<IGitRepositoriesExt>();
            gitExt.Clone(cloneUrl, clonePath, recurseSubmodules ? CloneOptions.RecurseSubmodule : CloneOptions.None);
        }

        public LibGit2Sharp.Repository GetActiveRepo()
        {
            var gitExt = serviceProvider.GetService<IGitExt>();
            if (gitExt.ActiveRepositories.Count > 0)
                return gitExt.ActiveRepositories.First().GetRepoFromIGit();
            return serviceProvider.GetSolution().GetRepoFromSolution();
        }

        public string GetActiveRepoPath()
        {
            var gitExt = serviceProvider.GetService<IGitExt>();
            if (gitExt.ActiveRepositories.Count > 0)
                return gitExt.ActiveRepositories.First().RepositoryPath;
            else
            {
                var repo = serviceProvider.GetSolution().GetRepoFromSolution();
                if (repo != null)
                    return repo.Info.Path;
            }
            return string.Empty;
        }

        static string PokeTheRegistryForLocalClonePath()
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VisualStudio\14.0\TeamFoundation\GitSourceControl\General");
            return (string)key.GetValue("DefaultRepositoryPath", string.Empty, RegistryValueOptions.DoNotExpandEnvironmentNames);
        }

        public void ShowMessage(string message)
        {
            var manager = serviceProvider.TryGetService<ITeamExplorer>() as ITeamExplorerNotificationManager;
            if (manager != null)
                manager.ShowNotification(message, NotificationType.Information, NotificationFlags.None, null, default(Guid));
        }

        public void ShowWarning(string message)
        {
            var manager = serviceProvider.TryGetService<ITeamExplorer>() as ITeamExplorerNotificationManager;
            if (manager != null)
                manager.ShowNotification(message, NotificationType.Warning, NotificationFlags.None, null, default(Guid));
        }

        public void ShowError(string message)
        {
            var manager = serviceProvider.TryGetService<ITeamExplorer>() as ITeamExplorerNotificationManager;
            if (manager != null)
                manager.ShowNotification(message, NotificationType.Error, NotificationFlags.None, null, default(Guid));
        }

        static class GitCoreServices
        {
            const string ns = "Microsoft.TeamFoundation.Git.CoreServices.";
            const string scchost = "ISccServiceHost";
            const string sccservice = "ISccSettingsService";

            const string AssemblyName = "Microsoft.TeamFoundation.Git.CoreServices";
            static Assembly assembly;
            static Assembly Assembly
            {
                get
                {
                    if (assembly != null)
                        return assembly;
                    assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == AssemblyName);
                    Debug.Assert(assembly != null, string.Format(CultureInfo.InvariantCulture, "{0} is not loaded in the AppDomain. This might mean there's no git provider loaded yet.", AssemblyName));
                    return assembly;
                }
            }

            static Type iSccServiceHostType;
            static Type ISccServiceHostType
            {
                get
                {
                    if (iSccServiceHostType != null)
                        return iSccServiceHostType;
                    var asm = Assembly;
                    if (asm == null)
                        return null; // validation/debug.assert already done
                    iSccServiceHostType = asm.GetType(ns + scchost);
                    Debug.Assert(iSccServiceHostType != null, string.Format(CultureInfo.InvariantCulture, "'{0}' {1} not found in assembly '{2}'. We need to check if it's been moved in this version.",
                        ns + scchost, "type", asm.GetCustomAttributeValue<AssemblyFileVersionAttribute>("Version")));
                    return iSccServiceHostType;
                }
            }

            static Type iSccSettingsServiceType;
            static Type ISccSettingsServiceType
            {
                get
                {
                    if (iSccSettingsServiceType != null)
                        return iSccSettingsServiceType;
                    var asm = Assembly;
                    if (asm == null)
                        return null; // validation/debug.assert already done
                    iSccSettingsServiceType = asm.GetType(ns + sccservice);
                    Debug.Assert(iSccSettingsServiceType != null, string.Format(CultureInfo.InvariantCulture, "'{0}' {1} not found in assembly '{2}'. We need to check if it's been moved in this version.",
                        ns + sccservice, "type", asm.GetCustomAttributeValue<AssemblyFileVersionAttribute>("Version")));
                    return iSccSettingsServiceType;
                }
            }

            static IServiceProvider GetISccServiceHost(IServiceProvider provider)
            {
                var type = ISccServiceHostType;
                if (type == null)
                    return null; // validation/debug.assert already done
                var ret = provider.TryGetService(type) as IServiceProvider;
                Debug.Assert(ret != null, string.Format(CultureInfo.InvariantCulture, "'{0}' {1} not found in assembly '{2}'. We need to check if it's been moved in this version.",
                    type, "service", assembly.GetCustomAttributeValue<AssemblyFileVersionAttribute>("Version")));
                return ret;
            }

            public static ISccSettingsService GetISccSettingsService(IServiceProvider provider)
            {
                var type = ISccSettingsServiceType;
                if (type == null)
                    return null; // validation/debug.assert already done
                var host = GetISccServiceHost(provider);
                if (host == null)
                    return null; // validation/debug.assert already done
                var ret = host.TryGetService(type);
                Debug.Assert(ret != null, string.Format(CultureInfo.InvariantCulture, "'{0}' {1} not found in assembly '{2}'. We need to check if it's been moved in this version.",
                    type, "service", assembly.GetCustomAttributeValue<AssemblyFileVersionAttribute>("Version")));
                return new ISccSettingsService(ret);
            }

            public class ISccSettingsService
            {
                readonly object settings;
                public ISccSettingsService(object provider)
                {
                    settings = provider;
                }

                public string DefaultRepositoryPath
                {
                    get
                    {
                        return ISccSettingsServiceType.GetValueForProperty(settings, "DefaultRepositoryPath") as string;
                    }
                }
            }
        }
    }
}