using System;
using System.Windows;
using System.Windows.Data;
using NullGuard;

namespace GitHub.UI
{
    public class ThicknessConverter : IValueConverter
    {
        public object Convert(
            object value,
            [AllowNull]Type targetType,
            [AllowNull]object parameter,
            [AllowNull]System.Globalization.CultureInfo culture)
        {
            var t = ((Thickness)value);

            return (t.Left + t.Right + t.Top + t.Bottom) / 4;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}