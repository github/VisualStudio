using System;
using System.Diagnostics;
using System.Globalization;

namespace GitHub.UI
{
    public class DefaultValueConverter : ValueConverterMarkupExtension<DefaultValueConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? parameter;
        }
    }
}
