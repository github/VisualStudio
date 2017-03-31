using System;
using System.Windows;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.VisualStudio.PlatformUI;
using GitHub.VisualStudio.Helpers;
using GitHub.Helpers;

namespace GitHub.VisualStudio.UI.Helpers
{
    public class SharedDictionaryManager : SharedDictionaryManagerBase
    {
        static bool isInDesignMode;

        static SharedDictionaryManager()
        {
            isInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());
        }

        public SharedDictionaryManager()
        {
            if (!isInDesignMode)
            {
                currentTheme = Colors.DetectTheme();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        string currentTheme;

        static readonly Dictionary<Uri, ResourceDictionary> resourceDicts = new Dictionary<Uri, ResourceDictionary>();
        static string baseThemeUri = "pack://application:,,,/GitHub.VisualStudio.UI;component/Styles/";

        Uri sourceUri;
        bool themed = false;
        public new Uri Source
        {
            get { return sourceUri; }
            set
            {
                if (!isInDesignMode)
                {
                    if (value.ToString() == "pack://application:,,,/GitHub.VisualStudio.UI;component/Styles/ThemeDesignTime.xaml")
                    {
                        if (!themed)
                        {
                            themed = true;
                            VSColorTheme.ThemeChanged += OnThemeChange;
                        }
                        value = new Uri(baseThemeUri + "Theme" + currentTheme + ".xaml");
                    }
                }

                sourceUri = value;
                ResourceDictionary ret;
                if (resourceDicts.TryGetValue(value, out ret))
                {
                    if (ret != this)
                    {
                        MergedDictionaries.Add(ret);
                        return;
                    }
                }
                base.Source = value;
                if (ret == null)
                    resourceDicts.Add(value, this);
            }
        }

        void OnThemeChange(ThemeChangedEventArgs e)
        {
            var uri = new Uri(baseThemeUri + "Theme" + currentTheme + ".xaml");
            ResourceDictionary ret;
            if (resourceDicts.TryGetValue(uri, out ret))
                MergedDictionaries.Remove(ret);
            currentTheme = Colors.DetectTheme();
            Source = uri;
        }
    }
}