using System.Windows;
using System.Windows.Media;

#pragma warning disable CA1720 // Identifier contains type name

namespace GitHub.UI
{
    public class ImageButton : DependencyObject
    {
        public static readonly DependencyProperty ImagePressedProperty =
            DependencyProperty.Register("ImagePressed", typeof(ImageSource), typeof(ImageButton), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ImageMouseOverProperty =
            DependencyProperty.Register("ImageMouseOver", typeof(ImageSource), typeof(ImageButton), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(ImageButton), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static ImageSource GetImage(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(ImageProperty);
        }

        public static ImageSource GetImageMouseOver(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(ImageMouseOverProperty);
        }

        public static ImageSource GetImagePressed(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(ImagePressedProperty);
        }

        public static void SetImage(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(ImageProperty, value);
        }

        public static void SetImageMouseOver(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(ImageMouseOverProperty, value);
        }

        public static void SetImagePressed(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(ImagePressedProperty, value);
        }
    }
}
