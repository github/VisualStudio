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
      "name": "EditorComments",
      "type": "bool",
      "default": "false"
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
      "name": "EnableTraceLogging",
      "type": "bool",
      "default": "false"
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

        bool editorComments;
        public bool EditorComments
        {
            get { return editorComments; }
            set { editorComments  = value; this.RaisePropertyChange(); }
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

        bool enableTraceLogging;
        public bool EnableTraceLogging
        {
            get { return enableTraceLogging; }
            set { enableTraceLogging  = value; this.RaisePropertyChange(); }
        }


        void LoadSettings()
        {
            CollectMetrics = (bool)SettingsStore.Read("CollectMetrics", true);
            EditorComments = (bool)SettingsStore.Read("EditorComments", false);
            UIState = SimpleJson.DeserializeObject<UIState>((string)SettingsStore.Read("UIState", "{}"));
            HideTeamExplorerWelcomeMessage = (bool)SettingsStore.Read("HideTeamExplorerWelcomeMessage", false);
            EnableTraceLogging = (bool)SettingsStore.Read("EnableTraceLogging", false);
        }

        void SaveSettings()
        {
            SettingsStore.Write("CollectMetrics", CollectMetrics);
            SettingsStore.Write("EditorComments", EditorComments);
            SettingsStore.Write("UIState", SimpleJson.SerializeObject(UIState));
            SettingsStore.Write("HideTeamExplorerWelcomeMessage", HideTeamExplorerWelcomeMessage);
            SettingsStore.Write("EnableTraceLogging", EnableTraceLogging);
        }

    }
}