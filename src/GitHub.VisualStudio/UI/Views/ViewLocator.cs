using System;
using System.Globalization;
using System.Windows.Data;
using NullGuard;

namespace GitHub.VisualStudio.UI.Views
{
    /// <summary>
    /// Locates a view for a view model.
    /// </summary>
    [NullGuard(ValidationFlags.None)]
    public class ViewLocator : IValueConverter
    {
        /// <summary>
        /// Gets a view for a view model.
        /// </summary>
        /// <param name="value">The view model.</param>
        /// <param name="targetType">Unused.</param>
        /// <param name="parameter">Unused.</param>
        /// <param name="culture">Unused.</param>
        /// <returns>The view.</returns>
        /// <remarks>
        /// This currently uses a "convention over configuration" approach, but we should probably
        /// be integrating with the UI factory to get the view.
        /// </remarks>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            const string vmNamespace = "GitHub.ViewModels";
            const string viewNamespace = "GitHub.VisualStudio.UI.Views";

            if (value != null)
            {
                var ns = value.GetType().Namespace;
                var name = value.GetType().Name;

                if (ns == vmNamespace)
                {
                    var viewTypeName = viewNamespace + '.' + name.Replace("ViewModel", "View");
                    return Activator.CreateInstance(Type.GetType(viewTypeName));
                }

                return value.GetType().FullName;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
