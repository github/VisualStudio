using System;
using System.IO;

namespace GitHub.Helpers
{
    public class SharedDictionaryManagerBase : LoadingResourceDictionary
    {
        public new Uri Source
        {
            get { return base.Source; }
            set
            {
                value = FixDesignTimeUri(value);
                base.Source = value;
            }
        }

        public static Uri FixDesignTimeUri(Uri inUri)
        {
            if (inUri.Scheme != "file")
            {
                return inUri;
            }

            var localPath = inUri.LocalPath;
            if (!localPath.EndsWith(@"\SharedDictionary.xaml"))
            {
                return inUri;
            }

            var fileName = Path.GetFileName(localPath);
            var assemblyName = Path.GetFileName(Path.GetDirectoryName(localPath));
            return new Uri($"pack://application:,,,/{assemblyName};component/{fileName}");
        }
    }
}
