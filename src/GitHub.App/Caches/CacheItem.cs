using System;
using System.Globalization;
using System.Reactive.Linq;
using Akavache;
using GitHub.Extensions;

namespace GitHub.Caches
{
    public class CacheItem
    {
        public string Key { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public IObservable<T> Save<T>(IBlobCache cache, string key, DateTimeOffset? absoluteExpiration = null)
            where T : CacheItem
        {
            Guard.ArgumentNotNull(cache, nameof(cache));
            Guard.ArgumentNotEmptyString(key, nameof(key));

            var k = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", key, Key);
            return cache
                .InvalidateObject<T>(k)
                .Select(_ => cache.InsertObject(k, this, absoluteExpiration))
                .Select(_ => this as T);
        }
    }
}