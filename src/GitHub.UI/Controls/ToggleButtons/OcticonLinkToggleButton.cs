using System.Windows;
using System.Windows.Controls.Primitives;

namespace GitHub.UI
{
    public class OcticonLinkToggleButton : OcticonToggleButton
    {
        public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register(
            "IconWidth", typeof(double), typeof(OcticonLinkToggleButton), new FrameworkPropertyMetadata
            {
                DefaultValue = 16.0,
            }
        );

        public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register(
            "IconHeight", typeof(double), typeof(OcticonLinkToggleButton), new FrameworkPropertyMetadata
            {
                DefaultValue = 16.0,
            }
        );

        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }

        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }
    }
}