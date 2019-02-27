using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using Microsoft.Win32;
using Serilog;
using static System.FormattableString;

namespace GitHub.TeamFoundation
{
    internal class RegistryHelper
    {
        static readonly ILogger log = LogManager.ForContext<RegistryHelper>();

        static RegistryKey OpenGitKey(string path)
        {
            var keyName = Invariant($"Software\\Microsoft\\VisualStudio\\{MajorVersion}.0\\TeamFoundation\\GitSourceControl\\{path}");
            return Registry.CurrentUser.OpenSubKey(keyName, true);
        }

        internal static IEnumerable<LocalRepositoryModel> PokeTheRegistryForRepositoryList()
        {
            using (var key = OpenGitKey("Repositories"))
            {
                if (key == null)
                {
                    return Enumerable.Empty<LocalRepositoryModel>();
                }

                return key.GetSubKeyNames().Select(x =>
                {
                    using (var subkey = key.OpenSubKey(x))
                    {
                        try
                        {
                            var path = subkey?.GetValue("Path") as string;
                            if (path != null && Directory.Exists(path))
                                return GitService.GitServiceHelper.CreateLocalRepositoryModel(path);
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

        internal static string PokeTheRegistryForLocalClonePath()
        {
            using (var key = OpenGitKey("General"))
            {
                return (string)key?.GetValue("DefaultRepositoryPath", string.Empty, RegistryValueOptions.DoNotExpandEnvironmentNames);
            }
        }

        const string MRUKeyPath = "MRUSettingsLocalProjectLocationEntries";
        internal static string SetDefaultProjectPath(string path)
        {
            var newProjectDialogKeyPath = Invariant($"Software\\Microsoft\\VisualStudio\\{MajorVersion}.0\\NewProjectDialog");

            var old = String.Empty;
            try
            {
                var newProjectKey = Registry.CurrentUser.OpenSubKey(newProjectDialogKeyPath, true) ??
                                    Registry.CurrentUser.CreateSubKey(newProjectDialogKeyPath);

                if (newProjectKey == null)
                {
                    throw new GitHubLogicException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Could not open or create registry key '{0}'",
                            newProjectDialogKeyPath));
                }

                using (newProjectKey)
                {
                    var mruKey = newProjectKey.OpenSubKey(MRUKeyPath, true) ??
                                 Microsoft.Win32.Registry.CurrentUser.CreateSubKey(MRUKeyPath);

                    if (mruKey == null)
                    {
                        throw new GitHubLogicException(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                "Could not open or create registry key '{0}'",
                                MRUKeyPath));
                    }

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
                log.Error(ex, "Error setting the create project path in the registry");
            }
            return old;
        }

        // Major version number of the current devenv process
        static int MajorVersion => Process.GetCurrentProcess().MainModule.FileVersionInfo.FileMajorPart;
    }
}
