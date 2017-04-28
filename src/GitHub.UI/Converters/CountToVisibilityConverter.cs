using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using NullGuard;

namespace GitHub.UI
{
    /// <summary>
    /// Convert a count to visibility based on the following rule:
    /// * If count == 0, return Visibility.Visible
    /// * If count > 0, return Visibility.Collapsed
    /// </summary>
    public class CountToVisibilityConverter : ValueConverterMarkupExtension<CountToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, [AllowNull] object parameter, [AllowNull] CultureInfo culture)
        {
            return ((int)value > 0) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
