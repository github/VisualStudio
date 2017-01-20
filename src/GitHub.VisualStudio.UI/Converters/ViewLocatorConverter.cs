using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using GitHub.Exports;
using GitHub.Models;
using NullGuard;

namespace GitHub.VisualStudio.UI.Converters
{
    /// <summary>
    /// Locates a view for a view model.
    /// </summary>
    /// <remarks>
    /// A converter of this type is present in GitHub.VisualStudio.UI\SharedDictionary.xaml and
    /// is used by a default DataTemplate for finding a view for a view model. This is a variation
    /// on the MVVM Convention over Configuration pattern[1] which uses MEF to locate the view.
    /// [1] http://stackoverflow.com/questions/768304
    /// </remarks>
    [NullGuard(ValidationFlags.None)]
    public class ViewLocatorConverter : IValueConverter
    {
        private static IExportFactoryProvider factoryProvider;

        /// <summary>
        /// Converts a view model into a view.
        /// </summary>
        /// <param name="value">The view model.</param>
        /// <param name="targetType">Unused.</param>
        /// <param name="parameter">Unused.</param>
        /// <param name="culture">Unused.</param>
        /// <returns>
        /// A new instance of a view for the specified view model, or an error string if a view
        /// could not be located.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var exportViewModelAttribute = value.GetType().GetCustomAttributes(typeof(ExportViewModelAttribute), false)
                .OfType<ExportViewModelAttribute>()
                .FirstOrDefault();

            if (exportViewModelAttribute != null)
            {
                var factory = FactoryProvider?.ViewFactory?
                    .FirstOrDefault(x => x.Metadata.ViewType == exportViewModelAttribute?.ViewType);

                if (factory != null)
                {
                    var result = factory.CreateExport().Value;
                    result.DataContext = value;
                    return result;
                }
            }

            return $"Could not locate view for '{value.GetType()}'";
        }

        /// <summary>
        /// Not implemented in this class.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static IExportFactoryProvider FactoryProvider
        {
            get
            {
                if (factoryProvider == null)
                {
                    factoryProvider = Services.GitHubServiceProvider.TryGetService<IExportFactoryProvider>();
                }

                return factoryProvider;
            }
        }
    }
}
