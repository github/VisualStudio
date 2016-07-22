using NullGuard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace GitHub.UI
{
    public class OcticonButton : Button
    {
        public static readonly DependencyProperty IconRotationAngleProperty = DependencyProperty.Register(
            "IconRotationAngle", typeof(double), typeof(OcticonButton),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

        public double IconRotationAngle
        {
            get { return (double)GetValue(IconRotationAngleProperty); }
            set { SetValue(IconRotationAngleProperty, value); }
        }

        public static DependencyProperty IconProperty =
            OcticonPath.IconProperty.AddOwner(typeof(OcticonButton));

        public Octicon Icon
        {
            [return: AllowNull]
            get { return (Octicon)GetValue(OcticonPath.IconProperty); }
            set { SetValue(OcticonPath.IconProperty, value); }
        }
    }
}
