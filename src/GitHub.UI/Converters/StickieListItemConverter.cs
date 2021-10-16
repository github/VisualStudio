using System;
using System.Globalization;

namespace GitHub.UI
{
    public class StickieListItemConverter : MultiValueConverterMarkupExtension<StickieListItemConverter>
    {
        public override object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            return value[1];
        }
    }
}