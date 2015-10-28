using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using Akavache;
using NullGuard;

namespace GitHub.Caches
{
    public class CacheIndex
    {
        public static CacheIndex Create(string key)
        {
            return new CacheIndex { IndexKey = key };
        }

        public CacheIndex()
        {
            Keys = new List<string>();
            OldKeys = new List<string>();
        }

        public IObservable<CacheIndex> AddAndSave(IBlobCache cache, string indexKey, CacheItem item,
            DateTimeOffset? absoluteExpiration = null)
        {
            var k = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", IndexKey, item.Key);
            if (!Keys.Contains(k))
                Keys.Add(k);
            UpdatedAt = DateTimeOffset.UtcNow;
            return cache.InsertObject(IndexKey, this, absoluteExpiration)
                        .Select(x => this);
        }

        public static IObservable<CacheIndex> AddAndSaveToIndex(IBlobCache cache, string indexKey, CacheItem item,
            DateTimeOffset? absoluteExpiration = null)
        {
            return cache.GetOrCreateObject(indexKey, () => Create(indexKey))
                .Do(index =>
                {
                    var k = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", index.IndexKey, item.Key);
                    if (!index.Keys.Contains(k))
                        index.Keys.Add(k);
                    index.UpdatedAt = DateTimeOffset.UtcNow;
                })
                .SelectMany(index => cache.InsertObject(index.IndexKey, index, absoluteExpiration)
                .Select(x => index));
        }

        public IObservable<CacheIndex> Clear(IBlobCache cache, string indexKey, DateTimeOffset? absoluteExpiration = null)
        {
            OldKeys = Keys.ToList();
            Keys.Clear();
            UpdatedAt = DateTimeOffset.UtcNow;
            return cache
                .InvalidateObject<CacheIndex>(indexKey)
                .SelectMany(_ => cache.InsertObject(indexKey, this, absoluteExpiration))
                .Select(_ => this);
        }

        [AllowNull]
        public string IndexKey {[return: AllowNull] get; set; }
        public List<string> Keys { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public List<string> OldKeys { get; set; }
    }
}
