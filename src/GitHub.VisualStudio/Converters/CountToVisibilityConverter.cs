using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using NullGuard;

namespace GitHub.VisualStudio.Converters
{
    /// <summary>
    /// Convert a count to visibility based on the following rule:
    /// * If count == 0, return Visibility.Visible
    /// * If count > 0, return Visibility.Collapsed
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, [AllowNull] object parameter, [AllowNull] CultureInfo culture)
        {
            return ((int)value == 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        [return: AllowNull]
        public object ConvertBack(object value, Type targetType, [AllowNull] object parameter, [AllowNull] CultureInfo culture)
        {
            return null;
        }
    }
}
