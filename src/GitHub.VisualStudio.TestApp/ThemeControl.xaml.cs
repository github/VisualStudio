using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Microsoft;

namespace ThemedDialog.UI
{
    /// <summary>
    /// Interaction logic for ThemeControl.xaml
    /// </summary>
    public partial class ThemeControl : UserControl
    {
        ResourceDictionary previousThemeResources;

        public ThemeControl()
        {
            InitializeComponent();
        }

        void ThemeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeListBox.SelectedItem is Label label)
            {
                if (label.Content is string theme)
                {
                    if (TryFindResource($"Themes/{theme}") is ResourceDictionary themeResources)
                    {
                        var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
                        mergedDictionaries.Add(themeResources);

                        if(previousThemeResources != null)
                        {
                            mergedDictionaries.Remove(previousThemeResources);
                        }

                        previousThemeResources = themeResources;

                        FireThemeChangedEvent();
                    }
                }
            }
        }

        static void FireThemeChangedEvent()
        {
            var type = Type.GetType("Microsoft.VisualStudio.PlatformUI.VSColorTheme, Microsoft.VisualStudio.Shell.14.0, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            Assumes.NotNull(type);
            var themeChangedStorage = type.GetField("ThemeChangedStorage", BindingFlags.Static | BindingFlags.NonPublic);
            Assumes.NotNull(themeChangedStorage);
            if (themeChangedStorage.GetValue(null) is Delegate method)
            {
                method.DynamicInvoke(new object[] { null });
            }
        }
    }
}
