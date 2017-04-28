using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using SettingsStore = GitHub.Helpers.SettingsStore;
using GitHub.Settings;
using GitHub.Primitives;

namespace GitHub.VisualStudio.Settings
{
    [Export(typeof(IPackageSettings))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class PackageSettings : NotificationAwareObject, IPackageSettings
    {
        readonly SettingsStore settingsStore;

        [ImportingConstructor]
        public PackageSettings([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
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