// This is an automatically generated file, based on settings.json and PackageSettingsGen.tt
/* settings.json content:
{
    "settings": [
    {
        "name": "CollectMetrics",
        "type": "bool",
        "default": 'true'
    },
    {
        "name": "UIState",
        "type": "object",
        "typename": "UIState",
        "default": 'null'
    }
    ]
}
*/

using GitHub.Settings;
using GitHub.Primitives;
using GitHub.VisualStudio.Helpers;

namespace GitHub.VisualStudio.Settings {

    public partial class PackageSettings : NotificationAwareObject, IPackageSettings
    {

        bool collectMetrics;
        public bool CollectMetrics
        {
            get { return collectMetrics; }
            set { collectMetrics  = value; this.RaisePropertyChange(); }
        }

        UIState uIState;
        public UIState UIState
        {
            get { return uIState; }
            set { uIState  = value; this.RaisePropertyChange(); }
        }


        void LoadSettings()
        {
            CollectMetrics = (bool)settingsStore.Read("CollectMetrics", true);
            UIState = SimpleJson.DeserializeObject<UIState>((string)settingsStore.Read("UIState", "{}"));
        }

        void SaveSettings()
        {
            settingsStore.Write("CollectMetrics", CollectMetrics);
            settingsStore.Write("UIState", SimpleJson.SerializeObject(UIState));
        }

    }
}