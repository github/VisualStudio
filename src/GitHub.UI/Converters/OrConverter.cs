using System;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace GitHub.UI
{
    [Localizability(LocalizationCategory.NeverLocalize)]
    public sealed class OrConverter : MultiValueConverterMarkupExtension<OrConverter>
    {
        public override object Convert(
            object[] value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return value.Select(x => System.Convert.ToBoolean(x)).Any(x => x);
        }

        public override object[] ConvertBack(
            object value,
            Type[] targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
