using Microsoft.VisualStudio.PlatformUI;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

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

            var color = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);
            var b = color.GetBrightness();
            Dark = b > 0.5f;
            VSColorTheme.ThemeChanged += _ =>
            {
                color = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);
                b = color.GetBrightness();
                Dark = b > 0.5f;
            };
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
