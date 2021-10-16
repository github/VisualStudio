using System;
using System.Globalization;
using System.Windows;

namespace GitHub.UI
{
    [Localizability(LocalizationCategory.NeverLocalize)]
    public sealed class BooleanToInverseVisibilityConverter : ValueConverterMarkupExtension<BooleanToInverseVisibilityConverter>
    {
        public override object Convert(object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return value is bool && (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }

        public override object ConvertBack(object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return value is Visibility && (Visibility)value != Visibility.Visible;
        }
    }
}
