using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.VisualStudio;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using System.Diagnostics;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;

namespace GitHub.Services
{
    [Export(typeof(IVSServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VSServices : IVSServices
    {
        readonly IUIProvider serviceProvider;

        [ImportingConstructor]
        public VSServices(IUIProvider serviceProvider)
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
                VsOutputLogger.WriteLine(string.Format(CultureInfo.CurrentCulture, "Error loading the default cloning path from the registry '{0}'", ex));
            }
            return ret;
        }

        public void Clone(string cloneUrl, string clonePath, bool recurseSubmodules)
        {
            var gitExt = serviceProvider.GetService<IGitRepositoriesExt>();
            gitExt.Clone(cloneUrl, clonePath, recurseSubmodules ? CloneOptions.RecurseSubmodule : CloneOptions.None);
        }

        IGitRepositoryInfo GetRepoFromVS()
        {
            var gitExt = serviceProvider.GetService<IGitExt>();
            return gitExt.ActiveRepositories.FirstOrDefault();
        }

        public LibGit2Sharp.IRepository GetActiveRepo()
        {
            var repo = GetRepoFromVS();
            return repo != null
                ? serviceProvider.GetService<IGitService>().GetRepo(repo.RepositoryPath)
                : serviceProvider.GetSolution().GetRepoFromSolution();
        }

        public string GetActiveRepoPath()
        {
            string ret = null;
            var repo = GetRepoFromVS();
            if (repo != null)
                ret = repo.RepositoryPath;
            if (ret == null)
                ret = serviceProvider.GetSolution().GetRepoFromSolution()?.Info?.Path;
            return ret ?? String.Empty;
        }

        public IEnumerable<ISimpleRepositoryModel> GetKnownRepositories()
        {
            try
            {
                return PokeTheRegistryForRepositoryList();
            }
            catch (Exception ex)
            {
                VsOutputLogger.WriteLine(string.Format(CultureInfo.CurrentCulture, "Error loading the repository list from the registry '{0}'", ex));
                return Enumerable.Empty<ISimpleRepositoryModel>();
            }
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
                        try
                        {
                            var path = subkey?.GetValue("Path") as string;
                            if (path != null)
                                return new SimpleRepositoryModel(path);
                        }
                        catch (Exception)
                        {
                            // no sense spamming the log, the registry might have ton of stale things we don't care about
                        }
                        return null;
                    }
                })
                .Where(x => x != null)
                .ToList();
            }
        }

        static string PokeTheRegistryForLocalClonePath()
        {
            using (var key = OpenGitKey("General"))
            {
                return (string)key?.GetValue("DefaultRepositoryPath", string.Empty, RegistryValueOptions.DoNotExpandEnvironmentNames);
            }
        }

        const string NewProjectDialogKeyPath = @"Software\Microsoft\VisualStudio\14.0\NewProjectDialog";
        const string MRUKeyPath = "MRUSettingsLocalProjectLocationEntries";
        public string SetDefaultProjectPath(string path)
        {
            var old = String.Empty;
            try
            {
                var newProjectKey = Registry.CurrentUser.OpenSubKey(NewProjectDialogKeyPath, true) ??
                                    Registry.CurrentUser.CreateSubKey(NewProjectDialogKeyPath);
                Debug.Assert(newProjectKey != null, string.Format(CultureInfo.CurrentCulture, "Could not open or create registry key '{0}'", NewProjectDialogKeyPath));

                using (newProjectKey)
                {
                    var mruKey = newProjectKey.OpenSubKey(MRUKeyPath, true) ??
                                 Registry.CurrentUser.CreateSubKey(MRUKeyPath);
                    Debug.Assert(mruKey != null, string.Format(CultureInfo.CurrentCulture, "Could not open or create registry key '{0}'", MRUKeyPath));

                    using (mruKey)
                    {
                        // is this already the default path? bail
                        old = (string)mruKey.GetValue("Value0", string.Empty, RegistryValueOptions.DoNotExpandEnvironmentNames);
                        if (String.Equals(path.TrimEnd('\\'), old.TrimEnd('\\'), StringComparison.CurrentCultureIgnoreCase))
                            return old;

                        // grab the existing list of recent paths, throwing away the last one
                        var numEntries = (int)mruKey.GetValue("MaximumEntries", 5);
                        var entries = new List<string>(numEntries);
                        for (int i = 0; i < numEntries - 1; i++)
                        {
                            var val = (string)mruKey.GetValue("Value" + i, String.Empty, RegistryValueOptions.DoNotExpandEnvironmentNames);
                            if (!String.IsNullOrEmpty(val))
                                entries.Add(val);
                        }

                        newProjectKey.SetValue("LastUsedNewProjectPath", path);
                        mruKey.SetValue("Value0", path);
                        // bump list of recent paths one entry down
                        for (int i = 0; i < entries.Count; i++)
                            mruKey.SetValue("Value" + (i + 1), entries[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                VsOutputLogger.WriteLine(string.Format(CultureInfo.CurrentCulture, "Error setting the create project path in the registry '{0}'", ex));
            }
            return old;
        }

        public void ActivityLogMessage(string message)
        {
            var log = serviceProvider.GetActivityLog();
            if (log != null)
            {
                if (!ErrorHandler.Succeeded(log.LogEntry((UInt32)__ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION,
                            Info.ApplicationInfo.ApplicationSafeName, message)))
                    Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "Could not log message to activity log: {0}", message));
            }
        }

        public void ActivityLogError(string message)
        {
            var log = serviceProvider.GetActivityLog();
            if (log != null)
            {

                if (!ErrorHandler.Succeeded(log.LogEntry((UInt32)__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR,
                            Info.ApplicationInfo.ApplicationSafeName, message)))
                    Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "Could not log error to activity log: {0}", message));
            }
        }

        public void ActivityLogWarning(string message)
        {
            var log = serviceProvider.GetActivityLog();
            if (log != null)
            {
                if (!ErrorHandler.Succeeded(log.LogEntry((UInt32)__ACTIVITYLOG_ENTRYTYPE.ALE_WARNING,
                            Info.ApplicationInfo.ApplicationSafeName, message)))
                    Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "Could not log warning to activity log: {0}", message));
            }
        }
    }
}
