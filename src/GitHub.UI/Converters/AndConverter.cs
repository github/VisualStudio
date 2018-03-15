using System;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace GitHub.UI
{
    [Localizability(LocalizationCategory.NeverLocalize)]
    public sealed class AndConverter : MultiValueConverterMarkupExtension<AndConverter>
    {
        public override object Convert(
            object[] value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return value.Select(x => System.Convert.ToBoolean(x)).All(x => x);
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
