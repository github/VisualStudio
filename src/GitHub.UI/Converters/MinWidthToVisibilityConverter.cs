using System;
using System.Globalization;
using System.Windows;

namespace GitHub.UI
{
    public class MinWidthToVisibilityConverter : ValueConverterMarkupExtension<MinWidthToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var minWidth = System.Convert.ToDouble((string)parameter, NumberFormatInfo.InvariantInfo);
            var width = (double)value;

            return (width > 0 && width < minWidth) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
