using System;
using System.Windows;
using System.Collections.Generic;

namespace GitHub.Helpers
{
    public class SharedDictionaryManagerBase : ResourceDictionary
    {
        static IDictionary<Uri, ResourceDictionary> sharedDictionaries;

        static SharedDictionaryManagerBase()
        {
            sharedDictionaries = new Dictionary<Uri, ResourceDictionary>();
        }

        public virtual new Uri Source
        {
            get { return base.Source; }
            set
            {
                value = FixDesignTimeUri(value);
                var rd = GetResourceDictionary(value);
                MergedDictionaries.Clear();
                MergedDictionaries.Add(rd);
            }
        }

        ResourceDictionary GetResourceDictionary(Uri uri)
        {
            ResourceDictionary rd;
            if (!sharedDictionaries.TryGetValue(uri, out rd))
            {
                rd = new LoadingResourceDictionary { Source = uri };
                sharedDictionaries[uri] = rd;
            }

            return rd;
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
