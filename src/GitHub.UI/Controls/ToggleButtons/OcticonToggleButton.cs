using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace GitHub.UI
{
    public abstract class OcticonToggleButton: ToggleButton
    {
        const FrameworkPropertyMetadataOptions affectsRender = FrameworkPropertyMetadataOptions.AffectsRender;

        public static readonly DependencyProperty IconCheckedProperty = DependencyProperty.Register(
            "IconChecked", typeof(Octicon), typeof(OcticonToggleButton), new FrameworkPropertyMetadata
            {
                DefaultValue = Octicon.chevron_down,
                PropertyChangedCallback = IconCheckedChanged
            }
        );

        public static readonly DependencyProperty PathCheckedProperty = DependencyProperty.Register(
            "PathChecked", typeof(Geometry), typeof(OcticonToggleButton),
            new FrameworkPropertyMetadata(OcticonPath.GetGeometryForIcon(Octicon.chevron_down), affectsRender)
        );

        public static readonly DependencyProperty IconUncheckedProperty = DependencyProperty.Register(
            "IconUnchecked", typeof(Octicon), typeof(OcticonToggleButton), new FrameworkPropertyMetadata
            {
                DefaultValue = Octicon.chevron_up,
                PropertyChangedCallback = IconUncheckedChanged
            }
        );

        public static readonly DependencyProperty PathUncheckedProperty = DependencyProperty.Register(
            "PathUnchecked", typeof(Geometry), typeof(OcticonToggleButton),
            new FrameworkPropertyMetadata(OcticonPath.GetGeometryForIcon(Octicon.chevron_up), affectsRender)
        );

        public static readonly DependencyProperty IconIndeterminateProperty = DependencyProperty.Register(
            "IconIndeterminate", typeof(Octicon), typeof(OcticonToggleButton), new FrameworkPropertyMetadata
            {
                DefaultValue = Octicon.question,
                PropertyChangedCallback = IconIndeterminateChanged
            }
        );

        public static readonly DependencyProperty PathIndeterminateProperty = DependencyProperty.Register(
            "PathIndeterminate", typeof(Geometry), typeof(OcticonToggleButton),
            new FrameworkPropertyMetadata(OcticonPath.GetGeometryForIcon(Octicon.question), affectsRender)
        );

        public static readonly DependencyProperty IconRotationAngleProperty =
            OcticonCircleButton.IconRotationAngleProperty.AddOwner(typeof(OcticonToggleButton));

        public Octicon IconChecked
        {
            get { return (Octicon)GetValue(IconCheckedProperty); }
            set { SetValue(IconCheckedProperty, value); }
}

        public Geometry PathChecked
        {
            get { return (Geometry)GetValue(PathCheckedProperty); }
            set { SetValue(PathCheckedProperty, value); }
        }

        public Octicon IconUnchecked
        {
            get { return (Octicon)GetValue(IconUncheckedProperty); }
            set { SetValue(IconUncheckedProperty, value); }
        }

        public Geometry PathUnchecked
        {
            get { return (Geometry)GetValue(PathUncheckedProperty); }
            set { SetValue(PathUncheckedProperty, value); }
        }

        public Octicon IconIndeterminate
        {
            get { return (Octicon)GetValue(IconIndeterminateProperty); }
            set { SetValue(IconIndeterminateProperty, value); }
        }

        public Geometry PathIndeterminate
        {
            get { return (Geometry)GetValue(PathIndeterminateProperty); }
            set { SetValue(PathIndeterminateProperty, value); }
        }

        public double IconRotationAngle
        {
            get { return (double)GetValue(IconRotationAngleProperty); }
            set { SetValue(IconRotationAngleProperty, value); }
        }

        static void OnIconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e,
            DependencyProperty destinationPathProperty)
        {
            d.SetValue(destinationPathProperty, OcticonPath.GetGeometryForIcon((Octicon)e.NewValue));
        }

        private static void IconCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OnIconPropertyChanged(d, e, PathCheckedProperty);
        }

        private static void IconUncheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OnIconPropertyChanged(d, e, PathUncheckedProperty);
        }

        private static void IconIndeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OnIconPropertyChanged(d, e, PathIndeterminateProperty);
        }
    }
}
