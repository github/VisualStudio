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
    [Export(typeof(IPackageSettings))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PackgeSettingsDispatcher : IPackageSettings
    {
        readonly IPackageSettings packageSettings;

        [ImportingConstructor]
        public PackgeSettingsDispatcher([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            packageSettings = serviceProvider.GetService(typeof(IPackageSettings)) as IPackageSettings;
            packageSettings.PropertyChanged += PropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Save()
        {
            packageSettings.Save();
        }

        public bool CollectMetrics
        {
            get { return packageSettings.CollectMetrics; }
            set { packageSettings.CollectMetrics = value; }
        }

        public bool EditorComments
        {
            get { return packageSettings.EditorComments; }
            set { packageSettings.EditorComments = value; }
        }

        public bool EnableTraceLogging
        {
            get { return packageSettings.EnableTraceLogging; }
            set { packageSettings.EnableTraceLogging = value; }
        }

        public UIState UIState
        {
            get { return packageSettings.UIState; }
            set { packageSettings.UIState = value; }
        }

        public bool HideTeamExplorerWelcomeMessage
        {
            get { return packageSettings.HideTeamExplorerWelcomeMessage; }
            set { packageSettings.HideTeamExplorerWelcomeMessage = value; }
        }
    }

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