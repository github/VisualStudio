using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GitHub.UI
{
    public class TrimNewlinesConverter : ValueConverterMarkupExtension<TrimNewlinesConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            if (String.IsNullOrEmpty(text)) return null;
            return Regex.Replace(text, @"\t|\n|\r", "");
        }
    }
}
