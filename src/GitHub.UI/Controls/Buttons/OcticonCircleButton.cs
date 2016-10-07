using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using NullGuard;

namespace GitHub.UI
{
    public class OcticonCircleButton : OcticonButton
    {
        public static readonly DependencyProperty ShowSpinnerProperty = DependencyProperty.Register(
            "ShowSpinner", typeof(bool), typeof(OcticonCircleButton));

        public static readonly DependencyProperty IconForegroundProperty = DependencyProperty.Register(
            "IconForeground", typeof(Brush), typeof(OcticonCircleButton));

        public static readonly DependencyProperty ActiveBackgroundProperty = DependencyProperty.Register(
            "ActiveBackground", typeof(Brush), typeof(OcticonCircleButton));

        public static readonly DependencyProperty ActiveForegroundProperty = DependencyProperty.Register(
            "ActiveForeground", typeof(Brush), typeof(OcticonCircleButton));

        public static readonly DependencyProperty PressedBackgroundProperty = DependencyProperty.Register(
            "PressedBackground", typeof(Brush), typeof(OcticonCircleButton));

        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
            "IconSize", typeof(double), typeof(OcticonCircleButton), new FrameworkPropertyMetadata(16d, 
                FrameworkPropertyMetadataOptions.AffectsArrange | 
                FrameworkPropertyMetadataOptions.AffectsMeasure | 
                FrameworkPropertyMetadataOptions.AffectsRender));

        public bool ShowSpinner
        {
            get { return (bool)GetValue(ShowSpinnerProperty); }
            set { SetValue(ShowSpinnerProperty, value); }
        }

        public Brush IconForeground
        {
            [return: AllowNull]
            get { return (Brush)GetValue(IconForegroundProperty); }
            set { SetValue(IconForegroundProperty, value); }
        }

        public Brush ActiveBackground
        {
            [return: AllowNull]
            get { return (Brush)GetValue(ActiveBackgroundProperty); }
            set { SetValue(ActiveBackgroundProperty, value); }
        }

        public Brush ActiveForeground
        {
            [return: AllowNull]
            get { return (Brush)GetValue(ActiveForegroundProperty); }
            set { SetValue(ActiveForegroundProperty, value); }
        }

        public Brush PressedBackground
        {
            [return: AllowNull]
            get { return (Brush)GetValue(PressedBackgroundProperty); }
            set { SetValue(PressedBackgroundProperty, value); }
        }

        public double IconSize
        {
            get { return (double)GetValue(IconSizeProperty); }
            set { SetValue(IconSizeProperty, value); }
        }

        static OcticonCircleButton()
        {
            Path.DataProperty.AddOwner(typeof(OcticonCircleButton));
        }

        static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(Path.DataProperty, OcticonPath.GetGeometryForIcon((Octicon)e.NewValue));
        }
    }
}
