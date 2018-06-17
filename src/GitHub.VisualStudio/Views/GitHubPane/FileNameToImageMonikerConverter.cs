using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Data;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used in XAML")]
    internal sealed class FileNameToImageMonikerConverter : IValueConverter
    {
        private readonly IVsImageService2 imageService;

        public FileNameToImageMonikerConverter()
        {
            imageService = (IVsImageService2)Package.GetGlobalService(typeof(SVsImageService));
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return imageService.GetImageMonikerForFile((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
