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
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using GitHub.Models;
using System.Management;
using System.Security.Principal;

namespace GitHub.Services
{
    public interface IVSServices
    {
        string GetLocalClonePathFromGitProvider();
        void Clone(string cloneUrl, string clonePath, bool recurseSubmodules);
        string GetActiveRepoPath();
        LibGit2Sharp.Repository GetActiveRepo();
        IEnumerable<ISimpleRepositoryModel> GetKnownRepositories();

        void ShowMessage(string message);
        void ShowWarning(string message);
        void ShowError(string message);
        void ClearNotifications();

        void ActivityLogMessage(string message);
        void ActivityLogWarning(string message);
        void ActivityLogError(string message);
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
            var repo = serviceProvider.GetSolution().GetRepoFromSolution();
            return repo?.Info?.Path ?? string.Empty;
        }

        public IEnumerable<ISimpleRepositoryModel> GetKnownRepositories()
        {
            return PokeTheRegistryForRepositoryList();
        }

        const string TEGitKey = @"Software\Microsoft\VisualStudio\14.0\TeamFoundation\GitSourceControl";
        static RegistryKey OpenGitKey(string path)
        {
            return Registry.CurrentUser.OpenSubKey(TEGitKey + "\\" + path, true);
        }

        static IEnumerable<ISimpleRepositoryModel> PokeTheRegistryForRepositoryList()
        {
            using (var key = OpenGitKey("Repositories"))
            {
                return key.GetSubKeyNames().Select(x =>
                {
                    using (var subkey = key.OpenSubKey(x))
                    {
                        var path = subkey.GetValue("Path") as string;
                        if (path != null)
                        {
                            var uri = VisualStudio.Services.GetRepoFromPath(path)?.GetUriFromRepository();
                            var name = uri?.GetRepo();
                            if (name != null)
                            {
                                name = uri.GetUser() + "/" + name;
                                return new SimpleRepositoryModel(name, uri.ToHttps().ToString(), path);
                            }
                        }
                    }
                    return null;
                })
                .Where(x => x != null)
                .ToList();
            }
        }

        static string PokeTheRegistryForLocalClonePath()
        {
            using (var key = OpenGitKey("General"))
            {
                if (key == null) return null;
                return (string)key.GetValue("DefaultRepositoryPath", string.Empty, RegistryValueOptions.DoNotExpandEnvironmentNames);
            }
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

        public void ClearNotifications()
        {
            var manager = serviceProvider.TryGetService<ITeamExplorer>() as ITeamExplorerNotificationManager;
            if (manager != null)
                manager.ClearNotifications();
		}

        public void ActivityLogMessage(string message)
        {
            var log = VisualStudio.Services.GetActivityLog(serviceProvider);
            if (log != null)
            {
                if (!ErrorHandler.Succeeded(log.LogEntry((UInt32)__ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION,
                            Info.ApplicationInfo.ApplicationSafeName, message)))
                    Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "Could not log message to activity log: {0}", message));
            }
        }

        public void ActivityLogError(string message)
        {
            var log = VisualStudio.Services.GetActivityLog(serviceProvider);
            if (log != null)
            {

                if (!ErrorHandler.Succeeded(log.LogEntry((UInt32)__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR,
                            Info.ApplicationInfo.ApplicationSafeName, message)))
                    Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "Could not log error to activity log: {0}", message));
            }
        }

        public void ActivityLogWarning(string message)
        {
            var log = VisualStudio.Services.GetActivityLog(serviceProvider);
            if (log != null)
            {
                if (!ErrorHandler.Succeeded(log.LogEntry((UInt32)__ACTIVITYLOG_ENTRYTYPE.ALE_WARNING,
                            Info.ApplicationInfo.ApplicationSafeName, message)))
                    Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "Could not log warning to activity log: {0}", message));
            }
        }
    }
}