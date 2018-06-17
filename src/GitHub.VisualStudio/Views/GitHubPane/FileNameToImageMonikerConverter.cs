using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Data;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    internal sealed class FileNameToImageMonikerConverter : IValueConverter
    {
        private const string shellFileAssociations = "ShellFileAssociations";
        private const string defaultIconMoniker = "DefaultIconMoniker";
        private const string knownMonikersPrefix = "KnownMonikers.";

        private readonly Package package;

        public FileNameToImageMonikerConverter()
        {
            package = (Package)Services.GitHubServiceProvider.GetService(typeof(Package));
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

        private static ImageMoniker? TryParseImageMoniker(string str)
        {
            // This is the common case: KnownMonikers.Foo

            if (str.StartsWith(knownMonikersPrefix, StringComparison.Ordinal))
            {
                var propertyName = str.Substring(knownMonikersPrefix.Length);
                var property = typeof(KnownMonikers).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);

                return property?.GetValue(null) as ImageMoniker?;
            }

            // Custom icon - this will look like: cb4a8fc6-efe7-424a-b611-23adf22b568e:6

            ImageMoniker imageMoniker;
            if (ImagingUtilities.TryParseImageMoniker(str, out imageMoniker))
                return imageMoniker;

            return null;
        }
    }
}
