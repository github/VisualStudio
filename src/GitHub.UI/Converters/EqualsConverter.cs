using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace GitHub.UI
{
    public class EqualsConverter : MarkupExtension, IValueConverter
    {
        readonly string argument;

        public EqualsConverter(string value)
        {
            argument = value;
        }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null && argument == null)
                return true;
            else if (value == null || argument == null)
                return false;
            else
                return value.ToString() == argument;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
