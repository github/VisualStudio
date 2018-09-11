using System;
using System.Globalization;
using System.Windows;

namespace GitHub.UI
{
    [Localizability(LocalizationCategory.NeverLocalize)]
    public sealed class NullOrWhitespaceToVisibilityConverter : ValueConverterMarkupExtension<NullOrWhitespaceToVisibilityConverter>
    {
        public override object Convert(object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is string s)
            {
                return string.IsNullOrWhiteSpace(s) ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public override object ConvertBack(object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
