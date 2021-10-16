using System;
using System.Globalization;
using System.Windows;

namespace GitHub.UI
{
    [Localizability(LocalizationCategory.NeverLocalize)]
    public sealed class EqualityConverter : MultiValueConverterMarkupExtension<EqualityConverter>
    {
        public override object Convert(
            object[] value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value.Length == 2)
            {
                return Equals(value[0], value[1]);
            }

            return false;
        }

        public override object[] ConvertBack(
            object value,
            Type[] targetType,
            object parameter,
            CultureInfo culture)
        {
            return null;
        }
    }
}
