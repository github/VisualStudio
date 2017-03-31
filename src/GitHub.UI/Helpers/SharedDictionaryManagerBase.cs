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

            var url = inUri.ToString();
            var assemblyPrefix = "/src/";
            var assemblyIndex = url.LastIndexOf(assemblyPrefix);
            if(assemblyIndex == -1)
            {
                return inUri;
            }

            assemblyIndex += assemblyPrefix.Length;
            var pathIndex = url.IndexOf('/', assemblyIndex);
            if(pathIndex == -1)
            {
                return inUri;
            }

            var assemblyName = url.Substring(assemblyIndex, pathIndex - assemblyIndex);
            var path = url.Substring(pathIndex + 1);

            return new Uri($"pack://application:,,,/{assemblyName};component/{path}");
        }
    }
}
