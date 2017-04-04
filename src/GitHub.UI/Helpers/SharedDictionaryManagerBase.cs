using System;
using System.Windows;
using System.Collections.Generic;

namespace GitHub.Helpers
{
    public class SharedDictionaryManagerBase : ResourceDictionary
    {
        static IDictionary<Uri, ResourceDictionary> sharedDictionaries;
        static IList<IDisposable> disposables;
        Uri source;

        static SharedDictionaryManagerBase()
        {
            // Avoid leaking resources at design time.
            sharedDictionaries = GetAppDomainSharedDictionaries();
            disposables = GetAppDomainDisposables();
        }

        public virtual new Uri Source
        {
            get { return source; }
            set
            {
                source = value;

                value = FixDesignTimeUri(value);
                var rd = GetResourceDictionary(value);
                MergedDictionaries.Clear();
                MergedDictionaries.Add(rd);

                // Remember dictionaries that need disposing of.
                var disposable = this as IDisposable;
                if (disposable != null && !disposables.Contains(disposable))
                {
                    disposables.Add(disposable);
                }
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
            if (assemblyIndex == -1)
            {
                return inUri;
            }

            assemblyIndex += assemblyPrefix.Length;
            var pathIndex = url.IndexOf('/', assemblyIndex);
            if (pathIndex == -1)
            {
                return inUri;
            }

            var assemblyName = url.Substring(assemblyIndex, pathIndex - assemblyIndex);
            var path = url.Substring(pathIndex + 1);

            return new Uri($"pack://application:,,,/{assemblyName};component/{path}");
        }

        static IDictionary<Uri, ResourceDictionary> GetAppDomainSharedDictionaries()
        {
            var name = typeof(SharedDictionaryManagerBase).FullName + ".SharedDictionaries";
            var sharedDictionaries = GetAppDomainData<Dictionary<Uri, ResourceDictionary>>(name);

            sharedDictionaries.Clear();
            return sharedDictionaries;
        }

        static IList<IDisposable> GetAppDomainDisposables()
        {
            var name = typeof(SharedDictionaryManagerBase).FullName + ".Disposables";
            var disposables = GetAppDomainData<List<IDisposable>>(name);
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }

            disposables.Clear();
            return disposables;
        }

        static T GetAppDomainData<T>(string name) where T : new()
        {
            var data = (T)AppDomain.CurrentDomain.GetData(name);
            if (data == null)
            {
                data = new T();
                AppDomain.CurrentDomain.SetData(name, data);
            }

            return data;
        }
    }
}
