using System;
using System.Windows;
using System.Collections.Generic;
using GitHub.Extensions;
using System.Globalization;

namespace GitHub.UI.Helpers
{
    public class SharedDictionaryManager : ResourceDictionary
    {
        CachingFactory factory;
        Uri source;

        public SharedDictionaryManager() : this(GetCurrentDomainCachingFactory())
        {
        }

        public SharedDictionaryManager(CachingFactory factory)
        {
            Guard.ArgumentNotNull(factory, nameof(factory));

            this.factory = factory;
        }

        public virtual new Uri Source
        {
            // Just in case the designer checks this property.
            get
            {
                return source;
            }

            set
            {
                source = value;

                value = FixDesignTimeUri(value);
                var rd = factory.GetOrCreateResourceDictionary(this, value);
                MergedDictionaries.Clear();
                MergedDictionaries.Add(rd);
            }
        }

        public static CachingFactory GetCurrentDomainCachingFactory()
        {
            var dataName = typeof(CachingFactory).FullName;
            var data = AppDomain.CurrentDomain.GetData(dataName);

            var cachingFactory = data as CachingFactory;
            if (cachingFactory != null)
            {
                return cachingFactory;
            }

            var disposable = data as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }

            cachingFactory = new CachingFactory();
            AppDomain.CurrentDomain.SetData(dataName, cachingFactory);
            return cachingFactory;
        }

        public class CachingFactory : IDisposable
        {
            IDictionary<Uri, ResourceDictionary> sharedDictionaries;
            ISet<IDisposable> disposables;

            public CachingFactory()
            {
                sharedDictionaries = new Dictionary<Uri, ResourceDictionary>();
                disposables = new HashSet<IDisposable>();
            }

            public ResourceDictionary GetOrCreateResourceDictionary(ResourceDictionary owner, Uri uri)
            {
                Guard.ArgumentNotNull(owner, nameof(owner));
                Guard.ArgumentNotNull(uri, nameof(uri));

                TryAddDisposable(owner);

                ResourceDictionary rd;
                if (!sharedDictionaries.TryGetValue(uri, out rd))
                {
                    rd = new LoadingResourceDictionary { Source = uri };
                    sharedDictionaries[uri] = rd;
                }

                return rd;
            }

            // Remember subtypes that need disposing of.
            public void TryAddDisposable(object owner)
            {
                Guard.ArgumentNotNull(owner, nameof(owner));

                var disposable = owner as IDisposable;
                if (disposable != null)
                {
                    disposables.Add(disposable);
                }
            }

            bool disposed;
            void Dispose(bool disposing)
            {
                if (disposed) return;
                if (disposing)
                {
                    disposed = true;
                    foreach (var disposable in disposables)
                    {
                        disposable.Dispose();
                    }

                    disposables.Clear();
                    sharedDictionaries.Clear();
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        public static Uri FixDesignTimeUri(Uri inUri)
        {
            Guard.ArgumentNotNull(inUri, nameof(inUri));

            if (inUri.Scheme != "file")
            {
                return inUri;
            }

            var url = inUri.ToString();
            var assemblyPrefix = "/src/";
            var assemblyIndex = url.LastIndexOf(assemblyPrefix, StringComparison.OrdinalIgnoreCase);
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

            return new Uri(String.Format(CultureInfo.InvariantCulture, "pack://application:,,,/{0};component/{1}", assemblyName, path));
        }
    }
}
