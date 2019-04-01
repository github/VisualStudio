using System;
using System.Windows;
using System.Windows.Controls;

namespace ThemedDialog.UI
{
    /// <summary>
    /// Interaction logic for ThemeControl.xaml
    /// </summary>
    public partial class ThemeControl : UserControl
    {
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
                        Application.Current.Resources.MergedDictionaries.Add(themeResources);
                    }
                }
            }
        }
    }
}
