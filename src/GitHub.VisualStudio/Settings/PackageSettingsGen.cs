// This is an automatically generated file, based on settings.json and PackageSettingsGen.tt
/* settings.json content:
{
	"settings": [
		{
			"name": "CollectMetrics",
			"type": "bool",
			"default": 'true'
		}
	]
}
*/

using GitHub.Settings;

namespace GitHub.VisualStudio.Settings {

    public partial class PackageSettings : IPackageSettings
    {

        public bool CollectMetrics { get; set; }

		void LoadSettings()
		{
			CollectMetrics = (bool)settingsStore.Read("CollectMetrics", true);
		}

		void SaveSettings()
		{
			settingsStore.Write("CollectMetrics", CollectMetrics);
		}

	}
}