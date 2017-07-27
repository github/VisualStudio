using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.VisualStudio;
using Microsoft.Win32;

namespace GitHub.TeamFoundation
{
    internal class RegistryHelper
    {
        const string TEGitKey = @"Software\Microsoft\VisualStudio\15.0\TeamFoundation\GitSourceControl";
        static RegistryKey OpenGitKey(string path)
        {
            return Microsoft.Win32.Registry.CurrentUser.OpenSubKey(TEGitKey + "\\" + path, true);
        }

        internal static IEnumerable<ILocalRepositoryModel> PokeTheRegistryForRepositoryList()
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
                                return new LocalRepositoryModel(path);
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

        const string NewProjectDialogKeyPath = @"Software\Microsoft\VisualStudio\15.0\NewProjectDialog";
        const string MRUKeyPath = "MRUSettingsLocalProjectLocationEntries";
        internal static string SetDefaultProjectPath(string path)
        {
            var old = String.Empty;
            try
            {
                var newProjectKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(NewProjectDialogKeyPath, true) ??
                                    Microsoft.Win32.Registry.CurrentUser.CreateSubKey(NewProjectDialogKeyPath);

                if (newProjectKey == null)
                {
                    throw new GitHubLogicException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Could not open or create registry key '{0}'",
                            NewProjectDialogKeyPath));
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
                VsOutputLogger.WriteLine(string.Format(CultureInfo.CurrentCulture, "Error setting the create project path in the registry '{0}'", ex));
            }
            return old;
        }
    }
}
