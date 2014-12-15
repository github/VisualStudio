using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;

namespace GitHub.UI
{
    [Localizability(LocalizationCategory.NeverLocalize)]
    public sealed class VerticalOffsetToVisibilityConverter : ValueConverterMarkupExtension<VerticalOffsetToVisibilityConverter>
    {
        readonly System.Windows.Controls.BooleanToVisibilityConverter converter = new System.Windows.Controls.BooleanToVisibilityConverter();

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var offset = (double)value;

            return converter.Convert(offset > 0.0, targetType, parameter, culture);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
