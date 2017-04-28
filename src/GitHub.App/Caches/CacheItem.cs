using System;
using System.Globalization;
using System.Reactive.Linq;
using Akavache;
using NullGuard;

namespace GitHub.Caches
{
    public class CacheItem
    {
        [AllowNull]
        public string Key {[return: AllowNull] get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public IObservable<T> Save<T>(IBlobCache cache, string key, DateTimeOffset? absoluteExpiration = null)
            where T : CacheItem
        {
            var k = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", key, Key);
            return cache
                .InvalidateObject<T>(k)
                .Select(_ => cache.InsertObject(k, this, absoluteExpiration))
                .Select(_ => this as T);
        }
    }
}