// This is an automatically generated file, based on settings.json and PackageSettingsGen.tt
/* settings.json content:
{
  "settings": [
    {
      "name": "CollectMetrics",
      "type": "bool",
      "default": "true"
    },
    {
      "name": "UIState",
      "type": "object",
      "typename": "UIState",
      "default": "null"
    },
    {
      "name": "HideTeamExplorerWelcomeMessage",
      "type": "bool",
      "default": "false"
    },
    {
      "name": "WelcomeMessageLastSeen",
      "type": "DateTime",
      "default": "DateTime.MinValue"
    }
  ]
}            
*/

using GitHub.Settings;
using GitHub.Primitives;
using GitHub.VisualStudio.Helpers;
using System;

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

        bool hideTeamExplorerWelcomeMessage;
        public bool HideTeamExplorerWelcomeMessage
        {
            get { return hideTeamExplorerWelcomeMessage; }
            set { hideTeamExplorerWelcomeMessage  = value; this.RaisePropertyChange(); }
        }

        DateTime welcomeMessageLastSeen;
        public DateTime WelcomeMessageLastSeen
        {
            get { return welcomeMessageLastSeen; }
            set { welcomeMessageLastSeen  = value; this.RaisePropertyChange(); }
        }


        void LoadSettings()
        {
            CollectMetrics = (bool)settingsStore.Read("CollectMetrics", true);
            UIState = SimpleJson.DeserializeObject<UIState>((string)settingsStore.Read("UIState", "{}"));
            HideTeamExplorerWelcomeMessage = (bool)settingsStore.Read("HideTeamExplorerWelcomeMessage", false);
            WelcomeMessageLastSeen = (DateTime)settingsStore.Read("WelcomeMessageLastSeen", DateTime.MinValue);
        }

        void SaveSettings()
        {
            settingsStore.Write("CollectMetrics", CollectMetrics);
            settingsStore.Write("UIState", SimpleJson.SerializeObject(UIState));
            settingsStore.Write("HideTeamExplorerWelcomeMessage", HideTeamExplorerWelcomeMessage);
            settingsStore.Write("WelcomeMessageLastSeen", WelcomeMessageLastSeen);
        }

    }
}