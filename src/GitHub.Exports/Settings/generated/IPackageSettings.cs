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
      "type": "DateTimeOffset",
      "default": "DateTimeOffset.MinValue"
    }
  ]
}            
*/

using System;
using System.ComponentModel;

namespace GitHub.Settings
{
    public interface IPackageSettings : INotifyPropertyChanged
    {
        void Save();
        bool CollectMetrics { get; set; }
        UIState UIState { get; set; }
        bool HideTeamExplorerWelcomeMessage { get; set; }
        DateTimeOffset WelcomeMessageLastSeen { get; set; }
    }
}
