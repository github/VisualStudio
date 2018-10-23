using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Data;
using Microsoft.VisualStudio.Imaging;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used in XAML")]
    internal sealed class DirectoryIsExpandedToImageMonikerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? KnownMonikers.FolderOpened : KnownMonikers.FolderClosed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
