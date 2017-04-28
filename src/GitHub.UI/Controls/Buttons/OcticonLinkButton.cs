using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NullGuard;

namespace GitHub.UI
{
    public partial class OcticonLinkButton : OcticonButton
    {
        public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register("IconHeight", typeof(int), typeof(OcticonLinkButton),new PropertyMetadata(16));
        public int IconHeight
        {
            get { return (int)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }

        public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register("IconWidth", typeof(int), typeof(OcticonLinkButton), new PropertyMetadata(16));
        public int IconWidth
        {
            get { return (int)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }

        static OcticonLinkButton()
        {
            OcticonPath.IconProperty.AddOwner(typeof(OcticonLinkButton), new FrameworkPropertyMetadata(OnIconChanged));
            Path.DataProperty.AddOwner(typeof(OcticonLinkButton));
        }

        public OcticonLinkButton()
        {
            this.SetValue(Path.DataProperty, OcticonPath.GetGeometryForIcon(Icon));
        }

        static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(Path.DataProperty, OcticonPath.GetGeometryForIcon((Octicon)e.NewValue));
        }
    }
}