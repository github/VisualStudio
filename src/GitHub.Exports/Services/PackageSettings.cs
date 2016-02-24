using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using GitHub.VisualStudio;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using SettingsStore = GitHub.Helpers.SettingsStore;
using System.Diagnostics;

namespace GitHub.Services
{
    [Export(typeof(IPackageSettings))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PackageSettings : IPackageSettings
    {
        readonly SettingsStore settingsStore;

        readonly Dictionary<string, object> SettingsProperties = new Dictionary<string, object> {
            { "CollectMetrics", true },
        };

        Dictionary<string, object> settings = new Dictionary<string, object>();

        public bool CollectMetrics
        {
            get
            {
                object val;
                TryGetSetting(nameof(CollectMetrics), out val);
                return (bool)val;
            }
            set { settings[nameof(CollectMetrics)] = value; }
        }

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

        bool TryGetSetting(string name, out object value)
        {
            if (!settings.ContainsKey(name))
            {
                if (!SettingsProperties.ContainsKey(name))
                    Debug.Assert(false, "PackageSettings: " + name + " setting missing. Did you add this setting to the SettingsProperties dictionary?");
                else
                    Debug.Assert(false, "PackageSettings: " + name + " setting missing. Registry might be corrupt.");
#if !DEBUG
                VsOutputLogger.WriteLine("PackageSettings: " + name + " setting missing.");
#endif
                value = SettingsProperties[name]; // default value
                return false;
            }
            value = settings[name];
            return true;
        }

        void LoadSettings()
        {
            try
            {
                foreach (var kvp in SettingsProperties)
                {
                    settings[kvp.Key] = settingsStore.Read(kvp.Key, kvp.Value);
                }
            }
            catch (Exception ex)
            {
                VsOutputLogger.WriteLine("PackageSettings: Unable to load settings. {0}", ex);
            }
        }

        void SaveSettings()
        {
            try
            {
                foreach (var kvp in SettingsProperties)
                {
                    settingsStore.Write(kvp.Key, settings[kvp.Key]);
                }
            }
            catch (Exception ex)
            {
                VsOutputLogger.WriteLine("PackageSettings: Unable to load settings. {0}", ex);
            }
        }
    }
}