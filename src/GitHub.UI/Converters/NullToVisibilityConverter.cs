using System;
using System.Globalization;
using System.Windows;

namespace GitHub.UI
{
    [Localizability(LocalizationCategory.NeverLocalize)]
    public sealed class NullToVisibilityConverter : ValueConverterMarkupExtension<NullToVisibilityConverter>
    {
        readonly System.Windows.Controls.BooleanToVisibilityConverter converter = new System.Windows.Controls.BooleanToVisibilityConverter();

        public override object Convert(object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return converter.Convert(value != null, targetType, parameter, culture);
        }

        public override object ConvertBack(object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return converter.ConvertBack(value != null, targetType, parameter, culture);
        }
    }
}
