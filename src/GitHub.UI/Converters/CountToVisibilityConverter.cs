using System;
using System.Globalization;
using System.Windows;

namespace GitHub.UI
{
    /// <summary>
    /// Convert a count to visibility based on the following rule:
    /// * If count == 0, return Visibility.Visible
    /// * If count > 0, return Visibility.Collapsed
    /// </summary>
    public class CountToVisibilityConverter : ValueConverterMarkupExtension<CountToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int)value > 0) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
