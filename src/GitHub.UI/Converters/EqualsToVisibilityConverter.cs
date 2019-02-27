using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace GitHub.UI
{
    public class EqualsToVisibilityConverter : MarkupExtension, IValueConverter
    {
        readonly string visibleValue;

        public EqualsToVisibilityConverter(string visibleValue)
        {
            this.visibleValue = visibleValue;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == visibleValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
