using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using SettingsStore = GitHub.Helpers.SettingsStore;
using GitHub.Settings;
using GitHub.Primitives;

namespace GitHub.VisualStudio.Settings
{
    public partial class PackageSettings : NotificationAwareObject, IPackageSettings
    {
        readonly SettingsStore settingsStore;

        public PackageSettings(IServiceProvider serviceProvider)
        {
            var sm = new ShellSettingsManager(serviceProvider);
            settingsStore = new SettingsStore(sm.GetWritableSettingsStore(SettingsScope.UserSettings), Info.ApplicationInfo.ApplicationSafeName);
            LoadSettings();
        }

        public void Save()
        {
            SaveSettings();
        }
    }
}