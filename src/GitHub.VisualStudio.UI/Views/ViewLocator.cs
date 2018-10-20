using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using GitHub.Factories;
using GitHub.Models;
using GitHub.ViewModels;
using static System.FormattableString;

namespace GitHub.VisualStudio.Views
{
    /// <summary>
    /// Locates a view for a view model.
    /// </summary>
    /// <remarks>
    /// A converter of this type should be placed in the resources for top-level GHfVS controls and
    /// is used as a default DataTemplate for finding a view for a view model. This is a variation
    /// on the MVVM Convention over Configuration pattern[1], here using MEF to locate the view.
    /// [1] http://stackoverflow.com/questions/768304
    /// </remarks>
    public class ViewLocator : IValueConverter
    {
        private static IViewViewModelFactory factoryProvider;

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
            var exportViewModelAttribute = value.GetType().GetCustomAttributes(typeof(ExportAttribute), false)
                .OfType<ExportAttribute>()
                .Where(x => typeof(IViewModel).IsAssignableFrom(x.ContractType))
                .FirstOrDefault();

            if (exportViewModelAttribute != null)
            {
                var view = Factory?.CreateView(exportViewModelAttribute.ContractType);

                if (view != null)
                {
                    var result = view;
                    result.DataContext = value;
                    return result;
                }
            }

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Break();
            }
#endif

            return Invariant($"Could not locate view for '{value.GetType()}'");
        }

        /// <summary>
        /// Not implemented in this class.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static IViewViewModelFactory Factory
        {
            get
            {
                if (factoryProvider == null)
                {
                    factoryProvider = Services.GitHubServiceProvider.TryGetService<IViewViewModelFactory>();
                }

                return factoryProvider;
            }
        }
    }
}
