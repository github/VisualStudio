using System;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace GitHub.UI
{
    [Localizability(LocalizationCategory.NeverLocalize)]
    public sealed class MultiBooleanToVisibilityConverter : MultiValueConverterMarkupExtension<MultiBooleanToVisibilityConverter>
    {
        public override object Convert(
            object[] value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return value.OfType<bool>().All(x => x) ? Visibility.Visible : Visibility.Collapsed;
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
