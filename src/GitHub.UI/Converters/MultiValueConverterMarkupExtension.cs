using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace GitHub.UI
{
    /// <summary>
    /// Serves as a base class for value converters (IMultiValueConverter) which are also markup 
    /// extensions (MarkupExtension).
    /// </summary>
    public abstract class MultiValueConverterMarkupExtension<T> : MarkupExtension, IMultiValueConverter where T : class, IMultiValueConverter, new()
    {
        static T converter;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return converter ?? (converter = new T());
        }

        public abstract object Convert(object[] value, Type targetType, object parameter, CultureInfo culture);

        /// <summary>
        /// Only override this if this converter might be used with 2-way data binding.
        /// </summary>
        public virtual object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}