using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used in XAML")]
    internal sealed class FileNameToImageMonikerConverter : IValueConverter
    {
        private const string shellFileAssociations = "ShellFileAssociations";
        private const string defaultIconMoniker = "DefaultIconMoniker";

        private readonly Package package;
        private readonly IVsImageService2 imageService;

        public FileNameToImageMonikerConverter()
        {
            package = (Package)Services.GitHubServiceProvider.GetService(typeof(Package));
            imageService = (IVsImageService2)Package.GetGlobalService(typeof(SVsImageService));
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TryGetIcon((string)value) ?? KnownMonikers.Document;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private ImageMoniker? TryGetIcon(string fileName)
        {
            var extension = Path.GetExtension(fileName);

            if (string.IsNullOrEmpty(extension))
                return null;

            using (var key = package.ApplicationRegistryRoot.OpenSubKey(shellFileAssociations + "\\" + extension))
            {
                var str = key?.GetValue(defaultIconMoniker) as string;
                if (str != null)
                    return TryParseImageMoniker(str);
            }

            return null;
        }

        private ImageMoniker? TryParseImageMoniker(string str)
        {
            ImageMoniker imageMoniker;
            if (imageService.TryParseImageMoniker(str, out imageMoniker))
                return imageMoniker;

            return null;
        }
    }
}
