using System;
using System.Globalization;

namespace GitHub.UI
{
    public class NotEqualsConverter : EqualsConverter
    {
        public NotEqualsConverter(string value)
            : base(value)
        {
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)base.Convert(value, targetType, parameter, culture);
        }
    }
}
