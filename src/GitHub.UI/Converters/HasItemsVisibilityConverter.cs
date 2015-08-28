using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace GitHub.UI
{
    [Localizability(LocalizationCategory.NeverLocalize)]
    public sealed class HasItemsVisibilityConverter : ValueConverterMarkupExtension<HasItemsVisibilityConverter>
    {
        readonly BooleanToVisibilityConverter converter = new BooleanToVisibilityConverter();

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IList ic = value as IList;
            return converter.Convert(ic != null && ic.Count > 0, targetType, parameter, culture);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
