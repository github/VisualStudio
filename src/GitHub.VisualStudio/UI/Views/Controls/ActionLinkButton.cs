using Microsoft.VisualStudio.PlatformUI;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    public class ActionLinkButton : Button
    {
        public ActionLinkButton()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            OnThemeChanged();

            VSColorTheme.ThemeChanged += _ =>
            {
                OnThemeChanged();
            };
        }

        void OnThemeChanged()
        {
            var color = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);
            Debug.Assert(color != null, "The theme color EnvironmentColors.ToolWindowTextColorKey is null");
            if (color == null) return;
            var brightness = color.GetBrightness();
            Dark = brightness > 0.5f;
        }

        public bool Dark
        {
            get { return (bool)GetValue(DarkProperty); }
            set { SetValue(DarkProperty, value); }
        }

        public static DependencyProperty DarkProperty =
            DependencyProperty.Register("Dark", typeof(bool), typeof(ActionLinkButton));
    }
}
