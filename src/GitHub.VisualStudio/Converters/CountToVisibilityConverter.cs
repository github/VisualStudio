using NullGuard;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GitHub.VisualStudio.Converters
{
    public class CountToVisibilityConverter : IValueConverter
    {
        [return: AllowNull]
        public object Convert([AllowNull] object value, [AllowNull] Type targetType, [AllowNull] object parameter, [AllowNull] CultureInfo culture)
        {
            return ((int)value == 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        [return: AllowNull]
        public object ConvertBack([AllowNull] object value, [AllowNull] Type targetType, [AllowNull] object parameter, [AllowNull] CultureInfo culture)
        {
            return null;
        }
    }
}
