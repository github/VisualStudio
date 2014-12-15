using System;
using System.Globalization;
using System.Windows;

namespace GitHub.UI
{
    public class BooleanToFontWeightConverter : ValueConverterMarkupExtension<BooleanToFontWeightConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && (bool) value ? FontWeights.Bold : FontWeights.Normal;
        }
    }
}