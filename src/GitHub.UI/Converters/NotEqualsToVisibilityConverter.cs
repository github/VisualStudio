using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace GitHub.UI
{
    public class NotEqualsToVisibilityConverter : MarkupExtension, IValueConverter
    {
        readonly string collapsedValue;

        public NotEqualsToVisibilityConverter(string collapsedValue)
        {
            this.collapsedValue = collapsedValue;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() != collapsedValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
