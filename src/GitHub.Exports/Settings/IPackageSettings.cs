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
namespace GitHub.Settings
{
    public interface IPackageSettings
    {
        void Save();
        bool CollectMetrics { get; set; }
    }
}
